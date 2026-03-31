# uplink.NET Linux repro harness

This console app is meant to make the Linux/.NET 6+ native crash easier to reproduce with a specific `uplink.NET` NuGet version.
The underlying bug was reported against Linux on .NET 6+, while this harness itself targets `net8.0` so it can be built with the SDK setup already used in this repository.

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
