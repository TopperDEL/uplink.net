using System.Runtime.InteropServices;
using uplink.NET.Models;
using uplink.NET.Services;

Settings settings;

try
{
    settings = Settings.Parse(args);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Settings.PrintUsage();
    return 2;
}

if (settings.ShowHelp)
{
    Settings.PrintUsage();
    return 0;
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    Console.WriteLine("Warning: this repro is intended for Linux/WSL/Fly.io. Continuing anyway.");
}

Console.WriteLine($"Starting uplink.NET Linux repro on {RuntimeInformation.OSDescription} / {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Rounds={settings.Rounds}, ChurnPerRound={settings.ChurnPerRound}, ExerciseBucketListing={settings.ExerciseBucketListing}");

Access.SetTempDirectory(Path.GetTempPath());

var startedAt = DateTimeOffset.UtcNow;

for (var round = 1; round <= settings.Rounds; round++)
{
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}/{settings.Rounds}: creating primary access");

    using var primary = CreateAccess(settings);
    ForceManagedPressure();

    for (var churn = 1; churn <= settings.ChurnPerRound; churn++)
    {
        using var throwaway = CreateAccess(settings);
        _ = throwaway.Serialize();

        if (settings.ExerciseBucketListing && churn % settings.BucketListEvery == 0)
        {
            await TryListBucketsAsync(throwaway, "throwaway", round, churn);
        }

        if (churn % settings.StatusEvery == 0 || churn == settings.ChurnPerRound)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: churned {churn}/{settings.ChurnPerRound} accesses");
        }
    }

    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: using primary access after churn");
    var serialized = primary.Serialize();
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: serialize() returned {serialized.Length} characters");

    if (settings.ExerciseBucketListing)
    {
        await TryListBucketsAsync(primary, "primary", round, settings.ChurnPerRound);
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

Console.WriteLine($"Completed without a native crash after {(DateTimeOffset.UtcNow - startedAt).TotalSeconds:F1}s.");
Console.WriteLine("If the old package did not segfault yet, rerun with larger --rounds and/or --churn values.");
return 0;

static Access CreateAccess(Settings settings)
{
    if (!string.IsNullOrWhiteSpace(settings.AccessGrant))
    {
        return new Access(settings.AccessGrant);
    }

    return new Access(settings.Satellite!, settings.ApiKey!, settings.Secret!);
}

static async Task TryListBucketsAsync(Access access, string label, int round, int churn)
{
    try
    {
        var bucketService = new BucketService(access);
        var bucketList = await bucketService.ListBucketsAsync(new ListBucketsOptions()).ConfigureAwait(false);
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}, {label} access after churn {churn}: listed {bucketList.Items.Count} buckets");

        foreach (var bucket in bucketList.Items)
        {
            bucket.Dispose();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}, {label} access after churn {churn}: bucket listing failed non-fatally: {ex.Message}");
    }
}

static void ForceManagedPressure()
{
    var buffers = new byte[32][];
    for (var i = 0; i < buffers.Length; i++)
    {
        buffers[i] = GC.AllocateUninitializedArray<byte>(64 * 1024);
        buffers[i][0] = (byte)i;
    }
}

internal sealed class Settings
{
    public string? AccessGrant { get; private init; }
    public string? Satellite { get; private init; }
    public string? ApiKey { get; private init; }
    public string? Secret { get; private init; }
    public int Rounds { get; private init; } = 10;
    public int ChurnPerRound { get; private init; } = 100;
    public int StatusEvery { get; private init; } = 25;
    public int BucketListEvery { get; private init; } = 25;
    public bool ExerciseBucketListing { get; private init; }
    public bool ShowHelp { get; private init; }

    public static Settings Parse(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help":
                case "-h":
                    flags.Add("help");
                    break;
                case "--list-buckets":
                    flags.Add("list-buckets");
                    break;
                case "--access-grant":
                case "--satellite":
                case "--api-key":
                case "--secret":
                case "--rounds":
                case "--churn":
                case "--status-every":
                case "--bucket-list-every":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException($"Missing value for {arg}.");
                    }

                    values[arg] = args[++i];
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}");
            }
        }

        var settings = new Settings
        {
            ShowHelp = flags.Contains("help"),
            AccessGrant = FirstNonEmpty(values, "--access-grant", "UPLINK_ACCESS_GRANT"),
            Satellite = FirstNonEmpty(values, "--satellite", "UPLINK_SATELLITE"),
            ApiKey = FirstNonEmpty(values, "--api-key", "UPLINK_API_KEY"),
            Secret = FirstNonEmpty(values, "--secret", "UPLINK_SECRET"),
            Rounds = ParsePositiveInt(values, "--rounds", "UPLINK_REPRO_ROUNDS", 10),
            ChurnPerRound = ParsePositiveInt(values, "--churn", "UPLINK_REPRO_CHURN", 100),
            StatusEvery = ParsePositiveInt(values, "--status-every", "UPLINK_REPRO_STATUS_EVERY", 25),
            BucketListEvery = ParsePositiveInt(values, "--bucket-list-every", "UPLINK_REPRO_BUCKET_LIST_EVERY", 25),
            ExerciseBucketListing = flags.Contains("list-buckets") || ParseBool("UPLINK_REPRO_LIST_BUCKETS")
        };

        if (settings.ShowHelp)
        {
            return settings;
        }

        var hasGrant = !string.IsNullOrWhiteSpace(settings.AccessGrant);
        var hasCredentials = !string.IsNullOrWhiteSpace(settings.Satellite)
            && !string.IsNullOrWhiteSpace(settings.ApiKey)
            && !string.IsNullOrWhiteSpace(settings.Secret);

        if (!hasGrant && !hasCredentials)
        {
            throw new ArgumentException("Provide either --access-grant (or UPLINK_ACCESS_GRANT) or the satellite/api-key/secret trio.");
        }

        return settings;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
Usage:
  dotnet run --project /absolute/path/to/uplink.NET.Repro.csproj -p:UplinkNuGetVersion=<version> -- [options]

Options:
  --access-grant <grant>         Serialized access grant. Also supported via UPLINK_ACCESS_GRANT.
  --satellite <address>          Satellite address. Also supported via UPLINK_SATELLITE.
  --api-key <key>                API key. Also supported via UPLINK_API_KEY.
  --secret <passphrase>          Passphrase. Also supported via UPLINK_SECRET.
  --rounds <count>               Outer rounds. Default: 10.
  --churn <count>                Access objects created per round. Default: 100.
  --status-every <count>         Progress log interval. Default: 25.
  --bucket-list-every <count>    Run ListBucketsAsync every N churn iterations. Default: 25.
  --list-buckets                 Also exercise _project through BucketService.ListBucketsAsync.
  --help                         Show this message.

Exit behavior:
  Broken Linux packages typically terminate the process with SIGSEGV / exit code 139.
  A fixed package should complete all rounds and print a success message.
""");
    }

    private static string? FirstNonEmpty(Dictionary<string, string?> values, string argumentName, string environmentName)
    {
        if (values.TryGetValue(argumentName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var environmentValue = Environment.GetEnvironmentVariable(environmentName);
        return string.IsNullOrWhiteSpace(environmentValue) ? null : environmentValue;
    }

    private static int ParsePositiveInt(Dictionary<string, string?> values, string argumentName, string environmentName, int fallback)
    {
        string? value = null;
        if (values.TryGetValue(argumentName, out var argumentValue))
        {
            value = argumentValue;
        }
        else
        {
            value = Environment.GetEnvironmentVariable(environmentName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new ArgumentException($"Invalid positive integer for {argumentName}: {value}");
        }

        return parsed;
    }

    private static bool ParseBool(string environmentName)
    {
        var value = Environment.GetEnvironmentVariable(environmentName);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
