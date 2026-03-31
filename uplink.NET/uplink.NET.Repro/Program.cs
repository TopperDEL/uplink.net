using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

Access.SetTempDirectory(Path.GetTempPath());

if (settings.WorkerDownloadMode)
{
    return await RunDownloadWorkerAsync(settings);
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    Console.WriteLine($"Warning: this repro is intended for Linux/WSL/Fly.io but is running on {RuntimeInformation.OSDescription}. Continuing anyway.");
}

Console.WriteLine($"Starting uplink.NET Linux repro on {RuntimeInformation.OSDescription} / {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Rounds={settings.Rounds}, ChurnPerRound={settings.ChurnPerRound}, SerializeRepeats={settings.SerializeRepeats}, DisposeBatchSize={settings.DisposeBatchSize}, ReparseAfterSerialize={settings.ReparseAfterSerialize}, ExerciseBucketListing={settings.ExerciseBucketListing}, ExerciseObjectIo={settings.ExerciseObjectIo}, ObjectIoEveryRounds={settings.ObjectIoEveryRounds}, FileSizeRangeMb={settings.MinFileSizeMb}-{settings.MaxFileSizeMb}, ParallelDownloadProcesses={settings.ParallelDownloadProcesses}, Bucket={(string.IsNullOrWhiteSpace(settings.BucketName) ? "<first visible>" : settings.BucketName)}");

var startedAtUtc = DateTimeOffset.UtcNow;

for (var round = 1; round <= settings.Rounds; round++)
{
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}/{settings.Rounds}: creating primary access");

    using var primary = CreateAccess(settings);
    ApplyManagedMemoryPressure();
    var pendingDisposals = new List<Access>(settings.DisposeBatchSize);

    try
    {
        for (var churn = 1; churn <= settings.ChurnPerRound; churn++)
        {
            Access? throwaway = null;
            try
            {
                throwaway = CreateAccess(settings);
                var serializedThrowaway = SerializeRepeatedly(throwaway, settings.SerializeRepeats);

                if (settings.ReparseAfterSerialize)
                {
                    using var reparsed = new Access(serializedThrowaway);
                    _ = SerializeRepeatedly(reparsed, settings.SerializeRepeats);
                }

                if (settings.ExerciseBucketListing && churn % settings.BucketListEvery == 0)
                {
                    await TryListBucketsAsync(throwaway, "throwaway", round, churn);
                }

                pendingDisposals.Add(throwaway);
                throwaway = null;

                if (pendingDisposals.Count >= settings.DisposeBatchSize)
                {
                    DisposeBatch(pendingDisposals);
                    ApplyManagedMemoryPressure();
                }

                if (churn % settings.StatusEvery == 0 || churn == settings.ChurnPerRound)
                {
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: churned {churn}/{settings.ChurnPerRound} accesses");
                }
            }
            finally
            {
                throwaway?.Dispose();
            }
        }
    }
    finally
    {
        DisposeBatch(pendingDisposals);
    }

    if (settings.ExerciseObjectIo && round % settings.ObjectIoEveryRounds == 0)
    {
        await TryExerciseObjectIoAsync(primary, settings, round);
    }

    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: using primary access after churn");
    var serialized = SerializeRepeatedly(primary, settings.SerializeRepeats);
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: serialize() returned {serialized.Length} characters");

    if (settings.ExerciseBucketListing)
    {
        await TryListBucketsAsync(primary, "primary", round, settings.ChurnPerRound);
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

Console.WriteLine($"Completed without a native crash after {(DateTimeOffset.UtcNow - startedAtUtc).TotalSeconds:F1}s.");
Console.WriteLine("If the old package did not segfault yet, rerun with larger --rounds/--churn values or enable more object I/O stress.");
return 0;

static Access CreateAccess(Settings settings)
{
    if (!string.IsNullOrWhiteSpace(settings.AccessGrant))
    {
        return new Access(settings.AccessGrant);
    }

    return new Access(settings.Satellite!, settings.ApiKey!, settings.Secret!);
}

static string SerializeRepeatedly(Access access, int count)
{
    string serialized = string.Empty;
    for (var i = 0; i < count; i++)
    {
        serialized = access.Serialize();
    }

    return serialized;
}

static async Task TryExerciseObjectIoAsync(Access access, Settings settings, int round)
{
    try
    {
        using var bucket = await TryResolveBucketAsync(access, settings, round).ConfigureAwait(false);
        if (bucket == null)
        {
            return;
        }

        var objectService = new ObjectService(access);
        var fileSizeBytes = PickRandomFileSizeBytes(settings);
        var objectKey = $"uplink-repro/{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.bin";
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"uplink-repro-{Guid.NewGuid():N}.bin");

        try
        {
            await CreateRandomFileAsync(tempFilePath, fileSizeBytes).ConfigureAwait(false);
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: generated random file {tempFilePath} ({fileSizeBytes / (1024d * 1024d):F1} MiB)");

            await using (var uploadStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true))
            using (var uploadOperation = await objectService.UploadObjectAsync(bucket, objectKey, new UploadOptions(), uploadStream, false).ConfigureAwait(false))
            {
                await (uploadOperation.StartUploadAsync() ?? Task.CompletedTask).ConfigureAwait(false);
                if (!uploadOperation.Completed || uploadOperation.Failed)
                {
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: upload failed non-fatally: {uploadOperation.ErrorMessage}");
                    return;
                }

                Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: uploaded {uploadOperation.BytesSent} bytes to {bucket.Name}/{objectKey}");
            }

            if (settings.ObjectIoWaitMs > 0)
            {
                await Task.Delay(settings.ObjectIoWaitMs).ConfigureAwait(false);
            }

            var serializedAccess = access.Serialize();
            var workers = StartDownloadWorkers(settings, serializedAccess, bucket.Name, objectKey, round);
            try
            {
                await MonitorWorkersWithListingsAsync(access, bucket, objectKey, settings, round, workers).ConfigureAwait(false);

                foreach (var worker in workers)
                {
                    await worker.WaitForExitAsync().ConfigureAwait(false);
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: download worker {worker.Id} exited with code {worker.ExitCode}");
                }
            }
            finally
            {
                foreach (var worker in workers)
                {
                    worker.Dispose();
                }
            }
        }
        finally
        {
            try
            {
                await objectService.DeleteObjectAsync(bucket, objectKey).ConfigureAwait(false);
                Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: deleted uploaded object {bucket.Name}/{objectKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: object cleanup failed non-fatally: {ex.Message}");
            }

            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: temp file cleanup failed non-fatally: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: object I/O stress failed non-fatally: {ex.Message}");
    }
}

static async Task<int> RunDownloadWorkerAsync(Settings settings)
{
    try
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Worker {settings.WorkerLabel}: starting download of {settings.WorkerBucketName}/{settings.WorkerObjectKey}");

        using var access = CreateAccess(settings);
        using var bucket = await GetBucketAsync(access, settings.WorkerBucketName!).ConfigureAwait(false);
        var objectService = new ObjectService(access);

        using var downloadOperation = await objectService.DownloadObjectAsync(bucket, settings.WorkerObjectKey!, new DownloadOptions(), false).ConfigureAwait(false);
        var downloadTask = downloadOperation.StartDownloadAsync() ?? Task.CompletedTask;
        var listingTask = KeepListingWhileDownloadingAsync(access, bucket, settings.WorkerObjectKey!, downloadOperation, settings, settings.WorkerLabel ?? "worker");

        await downloadTask.ConfigureAwait(false);
        await listingTask.ConfigureAwait(false);

        if (!downloadOperation.Completed || downloadOperation.Failed || downloadOperation.BytesReceived != downloadOperation.TotalBytes)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Worker {settings.WorkerLabel}: download failed: {downloadOperation.ErrorMessage}");
            return 1;
        }

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Worker {settings.WorkerLabel}: downloaded {downloadOperation.BytesReceived} bytes successfully");
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Worker {settings.WorkerLabel}: failed non-fatally: {ex.Message}");
        return 1;
    }
}

static async Task KeepListingWhileDownloadingAsync(Access access, Bucket bucket, string objectKey, DownloadOperation downloadOperation, Settings settings, string label)
{
    var objectService = new ObjectService(access);

    while (!downloadOperation.Completed && !downloadOperation.Failed && !downloadOperation.Cancelled)
    {
        await TryListBucketsAsync(access, $"{label}-worker", 0, 0).ConfigureAwait(false);
        await TryListObjectsAsync(objectService, bucket, objectKey, label, 0).ConfigureAwait(false);
        ApplyManagedMemoryPressure();

        await Task.Delay(settings.ObjectIoListingDelayMs).ConfigureAwait(false);
    }
}

static List<Process> StartDownloadWorkers(Settings settings, string serializedAccess, string bucketName, string objectKey, int round)
{
    var workers = new List<Process>(settings.ParallelDownloadProcesses);

    for (var workerIndex = 1; workerIndex <= settings.ParallelDownloadProcesses; workerIndex++)
    {
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine the current process path.");
        var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        if (string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entryAssemblyPath))
        {
            startInfo.FileName = processPath;
            startInfo.ArgumentList.Add(entryAssemblyPath);
        }
        else
        {
            startInfo.FileName = processPath;
        }

        startInfo.ArgumentList.Add("--worker-download");
        startInfo.ArgumentList.Add("--worker-bucket");
        startInfo.ArgumentList.Add(bucketName);
        startInfo.ArgumentList.Add("--worker-object-key");
        startInfo.ArgumentList.Add(objectKey);
        startInfo.ArgumentList.Add("--worker-label");
        startInfo.ArgumentList.Add($"round-{round}-worker-{workerIndex}");
        startInfo.ArgumentList.Add("--object-io-listing-delay-ms");
        startInfo.ArgumentList.Add(settings.ObjectIoListingDelayMs.ToString());
        startInfo.Environment["UPLINK_ACCESS_GRANT"] = serializedAccess;

        var worker = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the download worker process.");
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: started download worker PID={worker.Id} for {bucketName}/{objectKey}");
        workers.Add(worker);
    }

    return workers;
}

static async Task MonitorWorkersWithListingsAsync(Access access, Bucket bucket, string objectKey, Settings settings, int round, List<Process> workers)
{
    var objectService = new ObjectService(access);

    while (workers.Any(worker => !worker.HasExited))
    {
        await TryListBucketsAsync(access, "object-io-monitor", round, settings.ChurnPerRound).ConfigureAwait(false);
        await TryListObjectsAsync(objectService, bucket, objectKey, "object-io-monitor", round).ConfigureAwait(false);

        using var extraAccess = new Access(access.Serialize());
        _ = SerializeRepeatedly(extraAccess, settings.SerializeRepeats);
        ApplyManagedMemoryPressure();

        await Task.Delay(settings.ObjectIoListingDelayMs).ConfigureAwait(false);
    }
}

static async Task<Bucket?> TryResolveBucketAsync(Access access, Settings settings, int round)
{
    try
    {
        if (!string.IsNullOrWhiteSpace(settings.BucketName))
        {
            return await GetBucketAsync(access, settings.BucketName).ConfigureAwait(false);
        }

        var bucketService = new BucketService(access);
        var bucketList = await bucketService.ListBucketsAsync(new ListBucketsOptions()).ConfigureAwait(false);
        if (bucketList.Items.Count == 0)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: no visible buckets available for object I/O");
            return null;
        }

        Bucket? selectedBucket = null;
        foreach (var listedBucket in bucketList.Items)
        {
            if (selectedBucket == null)
            {
                selectedBucket = listedBucket;
                continue;
            }

            listedBucket.Dispose();
        }

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: using bucket {selectedBucket!.Name} for object I/O");
        return selectedBucket;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: could not resolve a bucket for object I/O: {ex.Message}");
        return null;
    }
}

static async Task<Bucket> GetBucketAsync(Access access, string bucketName)
{
    var bucketService = new BucketService(access);
    return await bucketService.GetBucketAsync(bucketName).ConfigureAwait(false);
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

static async Task TryListObjectsAsync(ObjectService objectService, Bucket bucket, string objectKey, string label, int round)
{
    try
    {
        var objectList = await objectService.ListObjectsAsync(bucket, new ListObjectsOptions()).ConfigureAwait(false);
        var foundObject = objectList.Items.Any(item => string.Equals(item.Key, objectKey, StringComparison.Ordinal));
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}, {label}: listed {objectList.Items.Count} objects in {bucket.Name}; target present={foundObject}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}, {label}: object listing failed non-fatally: {ex.Message}");
    }
}

static async Task CreateRandomFileAsync(string path, long fileSizeBytes)
{
    var buffer = new byte[1024 * 1024];
    long remaining = fileSizeBytes;

    await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, useAsync: true);
    while (remaining > 0)
    {
        var bytesToWrite = (int)Math.Min(buffer.Length, remaining);
        RandomNumberGenerator.Fill(buffer.AsSpan(0, bytesToWrite));
        await stream.WriteAsync(buffer.AsMemory(0, bytesToWrite)).ConfigureAwait(false);
        remaining -= bytesToWrite;
    }
}

static long PickRandomFileSizeBytes(Settings settings)
{
    var minBytes = settings.MinFileSizeMb * 1024L * 1024L;
    var maxBytes = settings.MaxFileSizeMb * 1024L * 1024L;
    return Random.Shared.NextInt64(minBytes, maxBytes + 1);
}

static void ApplyManagedMemoryPressure()
{
    var buffers = new byte[128][];
    for (var i = 0; i < buffers.Length; i++)
    {
        buffers[i] = GC.AllocateUninitializedArray<byte>(256 * 1024);
        buffers[i][0] = (byte)i;
    }
}

static void DisposeBatch(List<Access> accesses)
{
    for (var i = accesses.Count - 1; i >= 0; i--)
    {
        accesses[i].Dispose();
    }

    accesses.Clear();

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

internal sealed class Settings
{
    public string? AccessGrant { get; private init; }
    public string? Satellite { get; private init; }
    public string? ApiKey { get; private init; }
    public string? Secret { get; private init; }
    public string? BucketName { get; private init; }
    public string? WorkerBucketName { get; private init; }
    public string? WorkerObjectKey { get; private init; }
    public string? WorkerLabel { get; private init; }
    public int Rounds { get; private init; } = 10;
    public int ChurnPerRound { get; private init; } = 100;
    public int StatusEvery { get; private init; } = 25;
    public int BucketListEvery { get; private init; } = 25;
    public int SerializeRepeats { get; private init; } = 1;
    public int DisposeBatchSize { get; private init; } = 1;
    public int ObjectIoEveryRounds { get; private init; } = 1;
    public int MinFileSizeMb { get; private init; } = 1;
    public int MaxFileSizeMb { get; private init; } = 100;
    public int ParallelDownloadProcesses { get; private init; } = 2;
    public int ObjectIoWaitMs { get; private init; } = 2000;
    public int ObjectIoListingDelayMs { get; private init; } = 250;
    public bool ExerciseBucketListing { get; private init; }
    public bool ReparseAfterSerialize { get; private init; }
    public bool ExerciseObjectIo { get; private init; }
    public bool WorkerDownloadMode { get; private init; }
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
                case "--reparse-after-serialize":
                    flags.Add("reparse-after-serialize");
                    break;
                case "--object-io":
                    flags.Add("object-io");
                    break;
                case "--worker-download":
                    flags.Add("worker-download");
                    break;
                case "--access-grant":
                case "--satellite":
                case "--api-key":
                case "--secret":
                case "--bucket":
                case "--rounds":
                case "--churn":
                case "--status-every":
                case "--bucket-list-every":
                case "--serialize-repeats":
                case "--dispose-batch-size":
                case "--object-io-every-rounds":
                case "--min-file-size-mb":
                case "--max-file-size-mb":
                case "--parallel-download-processes":
                case "--object-io-wait-ms":
                case "--object-io-listing-delay-ms":
                case "--worker-bucket":
                case "--worker-object-key":
                case "--worker-label":
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
            BucketName = FirstNonEmpty(values, "--bucket", "UPLINK_REPRO_BUCKET"),
            WorkerBucketName = FirstNonEmpty(values, "--worker-bucket", "UPLINK_REPRO_WORKER_BUCKET"),
            WorkerObjectKey = FirstNonEmpty(values, "--worker-object-key", "UPLINK_REPRO_WORKER_OBJECT_KEY"),
            WorkerLabel = FirstNonEmpty(values, "--worker-label", "UPLINK_REPRO_WORKER_LABEL"),
            Rounds = ParsePositiveInt(values, "--rounds", "UPLINK_REPRO_ROUNDS", 10),
            ChurnPerRound = ParsePositiveInt(values, "--churn", "UPLINK_REPRO_CHURN", 100),
            StatusEvery = ParsePositiveInt(values, "--status-every", "UPLINK_REPRO_STATUS_EVERY", 25),
            BucketListEvery = ParsePositiveInt(values, "--bucket-list-every", "UPLINK_REPRO_BUCKET_LIST_EVERY", 25),
            SerializeRepeats = ParsePositiveInt(values, "--serialize-repeats", "UPLINK_REPRO_SERIALIZE_REPEATS", 1),
            DisposeBatchSize = ParsePositiveInt(values, "--dispose-batch-size", "UPLINK_REPRO_DISPOSE_BATCH_SIZE", 1),
            ObjectIoEveryRounds = ParsePositiveInt(values, "--object-io-every-rounds", "UPLINK_REPRO_OBJECT_IO_EVERY_ROUNDS", 1),
            MinFileSizeMb = ParsePositiveInt(values, "--min-file-size-mb", "UPLINK_REPRO_MIN_FILE_SIZE_MB", 1),
            MaxFileSizeMb = ParsePositiveInt(values, "--max-file-size-mb", "UPLINK_REPRO_MAX_FILE_SIZE_MB", 100),
            ParallelDownloadProcesses = ParsePositiveInt(values, "--parallel-download-processes", "UPLINK_REPRO_PARALLEL_DOWNLOAD_PROCESSES", 2),
            ObjectIoWaitMs = ParsePositiveInt(values, "--object-io-wait-ms", "UPLINK_REPRO_OBJECT_IO_WAIT_MS", 2000),
            ObjectIoListingDelayMs = ParsePositiveInt(values, "--object-io-listing-delay-ms", "UPLINK_REPRO_OBJECT_IO_LISTING_DELAY_MS", 250),
            ExerciseBucketListing = flags.Contains("list-buckets") || ParseBool("UPLINK_REPRO_LIST_BUCKETS"),
            ReparseAfterSerialize = flags.Contains("reparse-after-serialize") || ParseBool("UPLINK_REPRO_REPARSE_AFTER_SERIALIZE"),
            ExerciseObjectIo = flags.Contains("object-io") || ParseBool("UPLINK_REPRO_OBJECT_IO"),
            WorkerDownloadMode = flags.Contains("worker-download")
        };

        if (settings.ShowHelp)
        {
            return settings;
        }

        if (settings.MinFileSizeMb > settings.MaxFileSizeMb)
        {
            throw new ArgumentException("--min-file-size-mb cannot be larger than --max-file-size-mb.");
        }

        if (settings.WorkerDownloadMode)
        {
            if (string.IsNullOrWhiteSpace(settings.WorkerBucketName) || string.IsNullOrWhiteSpace(settings.WorkerObjectKey))
            {
                throw new ArgumentException("Worker download mode requires both --worker-bucket and --worker-object-key.");
            }
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
  --access-grant <grant>              Serialized access grant. Also supported via UPLINK_ACCESS_GRANT.
  --satellite <address>               Satellite address. Also supported via UPLINK_SATELLITE.
  --api-key <key>                     API key. Also supported via UPLINK_API_KEY.
  --secret <passphrase>               Passphrase. Also supported via UPLINK_SECRET.
  --bucket <name>                     Bucket to use for object I/O. Defaults to the first visible bucket. Also supported via UPLINK_REPRO_BUCKET.
  --rounds <count>                    Outer rounds. Default: 10.
  --churn <count>                     Access objects created per round. Default: 100.
  --status-every <count>              Progress log interval. Default: 25.
  --bucket-list-every <count>         Run ListBucketsAsync every N churn iterations. Default: 25.
  --serialize-repeats <count>         Serialize each access this many times before disposal. Default: 1.
  --dispose-batch-size <count>        Dispose throwaway accesses in batches of this size. Default: 1.
  --list-buckets                      Also exercise project-backed bucket operations through BucketService.ListBucketsAsync.
  --reparse-after-serialize           Reparse each serialized throwaway access and serialize it again before disposal.
  --object-io                         Generate a random 1-100 MiB file, upload it, wait, then spawn parallel download workers while listings continue.
  --object-io-every-rounds <count>    Run the object I/O stress every N rounds. Default: 1.
  --min-file-size-mb <count>          Minimum random object size in MiB. Default: 1.
  --max-file-size-mb <count>          Maximum random object size in MiB. Default: 100.
  --parallel-download-processes <n>   Number of worker processes that download the uploaded object in parallel. Default: 2.
  --object-io-wait-ms <count>         Delay after upload before parallel downloads begin. Default: 2000.
  --object-io-listing-delay-ms <n>    Delay between listing/serialize passes during object I/O stress. Default: 250.
  --help                              Show this message.

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
