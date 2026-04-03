# uplink.NET Linux repro harness

This console app is meant to make the Linux/.NET 6+ native crash easier to reproduce with a specific `uplink.NET` NuGet version by running a .NET 9 harness against it.
The underlying bug was reported against Linux on .NET 6+, and the harness itself now targets `net9.0` so you can stress the older package with a newer Linux runtime as well.

It intentionally:

1. creates a primary `Access`
2. creates many throwaway `Access` instances and can dispose them in bursts
3. repeatedly calls `Serialize()` on those instances to churn the same native result types
4. can reparse those serialized grants and serialize them again to double the native lifetime pressure
5. can generate multiple random 1-4 MiB files, upload them in parallel, wait, then start parallel download worker processes for each object while bucket/object listings continue
6. then uses the primary `Access` again

With the broken package, that usually ends in a native `SIGSEGV` / exit code `139` on Linux. With a package that includes the PR #51 fix, the process should finish cleanly.

## WSL / Linux usage

Use either a serialized access grant:

```bash
export UPLINK_ACCESS_GRANT='...'
dotnet run \
  --project <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3736 \
  -- \
  --rounds 10 \
  --churn 100 \
  --list-buckets
```

Or satellite credentials:

```bash
export UPLINK_SATELLITE='europe-west-1.tardigrade.io:7777'
export UPLINK_API_KEY='...'
export UPLINK_SECRET='...'
dotnet run \
  --project <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3736 \
  -- \
  --rounds 10 \
  --churn 100
```

If the crash does not show up quickly enough, raise `--rounds` and `--churn`.

If WSL still does not reproduce it, try much higher churn first, for example:

```bash
dotnet run \
  --project <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3736 \
  -- \
  --rounds 50 \
  --churn 500 \
  --bucket my-existing-bucket \
  --serialize-repeats 4 \
  --dispose-batch-size 32 \
  --status-every 50 \
  --bucket-list-every 10 \
  --object-io \
  --parallel-upload-objects 8 \
  --object-io-every-rounds 1 \
  --min-file-size-mb 1 \
  --max-file-size-mb 4 \
  --parallel-download-processes 8 \
  --object-io-wait-ms 2000 \
  --list-buckets \
  --reparse-after-serialize
```

## Testing a prerelease from PR #51

Point restore at the feed or folder that contains the prerelease packages and swap the version:

```bash
dotnet run \
  --project <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  --source /absolute/path/to/your/nuget/feed \
  -p:UplinkNuGetVersion=<your-prerelease-version> \
  -- \
  --rounds 10 \
  --churn 100 \
  --list-buckets
```

Expected result:

- old/broken package: process terminates with `SIGSEGV` / exit code `139`
- PR #51 prerelease: all rounds complete and the program prints the success message

## Fly.io

Files for Fly.io are included next to the repro project:

- `<absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/fly.toml`
- `<absolute-path-to-repo>/uplink.NET/uplink.NET.Repro/Dockerfile`

Before deploying, set your secrets:

```bash
cd <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro
fly secrets set UPLINK_ACCESS_GRANT='...'
```

Or use satellite credentials instead:

```bash
cd <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro
fly secrets set \
  UPLINK_SATELLITE='europe-west-1.tardigrade.io:7777' \
  UPLINK_API_KEY='...' \
  UPLINK_SECRET='...'
```

Then deploy:

```bash
cd <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro
fly deploy
fly logs
```

If you need a debug-symbol-enabled native library instead of the one from the NuGet package, place your local build at:

```bash
./uplink.NET/uplink.NET.Repro/docker-overrides/storj_uplink.so
```

Run `docker build` or `fly deploy` from `./uplink.NET/uplink.NET.Repro`. The Docker build copies that file into the published app after `dotnet publish`, replacing the NuGet-provided `/app/runtimes/linux-x64/native/storj_uplink.so` inside the image. If the override file is absent, the image keeps the default native library from the package.

Crash dumps are now configured by default in the included Fly config. The repro writes them to `/tmp/uplink-repro-crash-artifacts`. A lightweight supervisor process now runs each stress round in its own child process, so if that child segfaults the parent stays alive, retries crash-artifact scans for a few seconds, and uploads leftover dump/error files to Storj under `uplink-repro/crash-artifacts/...` in the `s-drive` bucket by default.

When a crash dump or `createdump` log is discovered, the supervisor now also creates a crash-analysis bundle under `/tmp/uplink-repro-crash-artifacts/bundle-<timestamp>-<pid>/`. That bundle is intended to make offline analysis on another Linux/WSL machine possible and includes:

- the detected dump/log/crash-report files plus `active-round*.json`
- targeted app files from `/app` such as `uplink.NET.Repro.dll`, `uplink.NET.dll`, matching `.pdb` files, config/json files, and `runtimes/linux-x64/native/storj_uplink.so`
- the current `dotnet` host plus the loaded/active CoreCLR runtime files (`libhostfxr.so`, `libhostpolicy.so`, `libcoreclr.so`, `libclrjit.so`, `System.Private.CoreLib.dll`, `System.Runtime.dll` when found)
- `analysis-manifest.json` and `analysis-manifest.txt` with host/runtime metadata, `dotnet --info`, copied file list, SHA256 hashes, and file sizes

The bundle logic waits for dump/log files to stop changing before copying them, only copies targeted debugging inputs, and avoids broad secret-heavy directories such as `/tmp` or `/root`.

By default the repro now enables:

- `DOTNET_DbgEnableMiniDump=1`
- `DOTNET_DbgMiniDumpType=4` for a full managed dump
- `DOTNET_DbgMiniDumpName=/tmp/uplink-repro-crash-artifacts/coredump.%p.%e.%h.%t.dmp`
- `DOTNET_EnableCrashReport=1` so the runtime also writes `*.crashreport.json`
- `DOTNET_CreateDumpDiagnostics=1`
- `DOTNET_CreateDumpVerboseDiagnostics=1`
- `DOTNET_CreateDumpLogToFile=/tmp/uplink-repro-crash-artifacts/createdump.%p.%e.%h.%t.log`

The container image now also installs `gdb`. When a crash dump/core file is found, the surviving supervisor tries a best-effort non-interactive `gdb` run against it, logs any `storj_uplink.so` frames it can resolve, writes a sibling `*.gdb-report.txt` file into `/tmp/uplink-repro-crash-artifacts`, and uploads that report to Storj together with the other crash artifacts/bundles.

On Linux the repro also raises `RLIMIT_CORE` on startup and starts supervised child/worker processes with `/tmp/uplink-repro-crash-artifacts` as their working directory. That gives regular kernel core files the best chance to land in the crash-artifact area too, subject to the host's `core_pattern` configuration.

You can override those locations if needed:

```bash
cd <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro
fly secrets set UPLINK_REPRO_BUCKET='your-existing-bucket'
fly secrets set UPLINK_REPRO_CRASH_ARTIFACT_BUCKET='s-drive'
fly secrets set UPLINK_REPRO_CRASH_ARTIFACT_PREFIX='uplink-repro/custom-crash-artifacts'
```

For local Linux/WSL runs, enable dump generation the same way before starting the repro:

```bash
export DOTNET_DbgEnableMiniDump=1
export DOTNET_DbgMiniDumpType=4
export DOTNET_DbgMiniDumpName=/tmp/uplink-repro-crash-artifacts/coredump.%p.%e.%h.%t.dmp
export DOTNET_EnableCrashReport=1
export DOTNET_CreateDumpDiagnostics=1
export DOTNET_CreateDumpVerboseDiagnostics=1
export DOTNET_CreateDumpLogToFile=/tmp/uplink-repro-crash-artifacts/createdump.%p.%e.%h.%t.log
```

The repro also writes `/tmp/uplink-repro-crash-artifacts/active-round.json` at the start of each round. If the child dies mid-round, that stale state file is uploaded by the surviving supervisor together with any dump files so you can see which round/process crashed and which dump/core settings were active at the time. The same stale round-state file is also copied into the generated crash-analysis bundle.

If you want the upload/download stress to target a specific bucket, set it as a secret or env var before deploying:

```bash
cd <absolute-path-to-repo>/uplink.NET/uplink.NET.Repro
fly secrets set UPLINK_REPRO_BUCKET='your-existing-bucket'
```

If `UPLINK_REPRO_BUCKET` is not set, the harness uses the first bucket visible to the supplied access grant.

The included Fly config uses more aggressive defaults than the local examples:

- `UPLINK_REPRO_ROUNDS=200`
- `UPLINK_REPRO_CHURN=2000`
- `UPLINK_REPRO_STATUS_EVERY=100`
- `UPLINK_REPRO_BUCKET_LIST_EVERY=10`
- `UPLINK_REPRO_SERIALIZE_REPEATS=4`
- `UPLINK_REPRO_DISPOSE_BATCH_SIZE=32`
- `UPLINK_REPRO_LIST_BUCKETS=true`
- `UPLINK_REPRO_REPARSE_AFTER_SERIALIZE=true`
- `UPLINK_REPRO_OBJECT_IO=true`
- `UPLINK_REPRO_OBJECT_IO_EVERY_ROUNDS=1`
- `UPLINK_REPRO_PARALLEL_UPLOAD_OBJECTS=8`
- `UPLINK_REPRO_MIN_FILE_SIZE_MB=1`
- `UPLINK_REPRO_MAX_FILE_SIZE_MB=4`
- `UPLINK_REPRO_PARALLEL_DOWNLOAD_PROCESSES=8`
- `UPLINK_REPRO_OBJECT_IO_WAIT_MS=2000`
- `UPLINK_REPRO_OBJECT_IO_LISTING_DELAY_MS=250`

If you need to test a PR #51 prerelease on Fly.io, edit `UPLINK_NUGET_VERSION` under `[build.args]` in `fly.toml` and deploy again.
