# uplink.NET Linux repro harness

This console app is meant to make the Linux/.NET 6+ native crash easier to reproduce with a specific `uplink.NET` NuGet version.
The underlying bug was reported against Linux on .NET 6+, and the same lifetime issue is what this harness is trying to surface; the harness itself now targets `net9.0` so you can stress the older package with a newer Linux runtime as well.

It intentionally:

1. creates a primary `Access`
2. creates and disposes many throwaway `Access` instances
3. repeatedly calls `Serialize()` on those instances to churn the same native result types
4. then uses the primary `Access` again

With the broken package, that usually ends in a native `SIGSEGV` / exit code `139` on Linux. With a package that includes the PR #51 fix, the process should finish cleanly.

## WSL / Linux usage

Use either a serialized access grant:

```bash
export UPLINK_ACCESS_GRANT='...'
dotnet run \
  --project /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3623 \
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
  --project /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3623 \
  -- \
  --rounds 10 \
  --churn 100
```

If the crash does not show up quickly enough, raise `--rounds` and `--churn`.

If WSL still does not reproduce it, try much higher churn first, for example:

```bash
dotnet run \
  --project /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
  -p:UplinkNuGetVersion=2.14.3623 \
  -- \
  --rounds 50 \
  --churn 500 \
  --status-every 50 \
  --bucket-list-every 25 \
  --list-buckets
```

## Testing a prerelease from PR #51

Point restore at the feed or folder that contains the prerelease packages and swap the version:

```bash
dotnet run \
  --project /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/uplink.NET.Repro.csproj \
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

- `/home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/fly.toml`
- `/home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro/Dockerfile`

Before deploying, set your secrets:

```bash
cd /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro
fly secrets set UPLINK_ACCESS_GRANT='...'
```

Or use satellite credentials instead:

```bash
cd /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro
fly secrets set \
  UPLINK_SATELLITE='europe-west-1.tardigrade.io:7777' \
  UPLINK_API_KEY='...' \
  UPLINK_SECRET='...'
```

Then deploy:

```bash
cd /home/runner/work/uplink.net/uplink.net/uplink.NET/uplink.NET.Repro
fly deploy
fly logs
```

The included Fly config uses more aggressive defaults than the local examples:

- `UPLINK_REPRO_ROUNDS=50`
- `UPLINK_REPRO_CHURN=500`
- `UPLINK_REPRO_STATUS_EVERY=50`
- `UPLINK_REPRO_BUCKET_LIST_EVERY=25`
- `UPLINK_REPRO_LIST_BUCKETS=true`

If you need to test a PR #51 prerelease on Fly.io, edit `UPLINK_NUGET_VERSION` under `[build.args]` in `fly.toml` and deploy again.
