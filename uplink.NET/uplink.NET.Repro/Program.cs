using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using uplink.NET.Models;
using uplink.NET.Services;

Settings settings;
var originalArgs = args;

try
{
    settings = Settings.Parse(args);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"{ex.Message} Use --help to see the supported options and environment variables.");
    Settings.PrintUsage();
    return 2;
}

if (settings.ShowHelp)
{
    Settings.PrintUsage();
    return 0;
}

Access.SetTempDirectory(Path.GetTempPath());
Directory.CreateDirectory(settings.CrashArtifactDirectory);
EnsureCrashDiagnosticsEnvironment(settings);
TryRaiseNativeCoreDumpLimit();
LogCrashDiagnosticsConfiguration(settings);

if (settings.WorkerDownloadMode)
{
    return await RunDownloadWorkerAsync(settings);
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    Console.WriteLine($"Warning: this repro is intended for Linux/WSL/Fly.io but is running on {RuntimeInformation.OSDescription}. Continuing anyway.");
}

Console.WriteLine($"Starting uplink.NET Linux repro on {RuntimeInformation.OSDescription} / {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Rounds={settings.Rounds}, ChurnPerRound={settings.ChurnPerRound}, SerializeRepeats={settings.SerializeRepeats}, DisposeBatchSize={settings.DisposeBatchSize}, ReparseAfterSerialize={settings.ReparseAfterSerialize}, ExerciseBucketListing={settings.ExerciseBucketListing}, ExerciseObjectIo={settings.ExerciseObjectIo}, ObjectIoEveryRounds={settings.ObjectIoEveryRounds}, FileSizeRangeMb={settings.MinFileSizeMb}-{settings.MaxFileSizeMb}, ParallelUploadObjects={settings.ParallelUploadObjects}, ParallelDownloadProcesses={settings.ParallelDownloadProcesses}, Bucket={(string.IsNullOrWhiteSpace(settings.BucketName) ? "<first visible>" : settings.BucketName)}");
Console.WriteLine($"CrashArtifactDirectory={settings.CrashArtifactDirectory}, CrashArtifactBucket={settings.CrashArtifactBucketName}, CrashArtifactPrefix={settings.CrashArtifactPrefix}");

if (!IsCrashDumpCollectionConfigured(settings))
{
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash dump collection is not explicitly enabled in the environment. Set DOTNET_DbgEnableMiniDump=1 and DOTNET_DbgMiniDumpName={settings.CrashArtifactDirectory}/coredump.%p.%e.%h.%t.dmp to capture dumps automatically.");
}

if (settings.SupervisorChildMode)
{
    return await RunSupervisorChildAsync(settings).ConfigureAwait(false);
}

return await RunSupervisorAsync(settings, originalArgs).ConfigureAwait(false);

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
        var stressObjects = await CreateStressObjectsAsync(settings, round).ConfigureAwait(false);

        try
        {
            await Task.WhenAll(stressObjects.Select(stressObject => UploadStressObjectAsync(objectService, bucket, stressObject, round))).ConfigureAwait(false);
            var uploadedObjects = stressObjects.Where(stressObject => stressObject.Uploaded).ToList();
            if (uploadedObjects.Count == 0)
            {
                Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: all parallel uploads failed non-fatally; skipping downloads");
                return;
            }

            if (settings.ObjectIoWaitMs > 0)
            {
                await Task.Delay(settings.ObjectIoWaitMs).ConfigureAwait(false);
            }

            var serializedAccess = access.Serialize();
            var workers = new List<Process>(uploadedObjects.Count * settings.ParallelDownloadProcesses);
            foreach (var uploadedObject in uploadedObjects)
            {
                workers.AddRange(StartDownloadWorkers(settings, serializedAccess, bucket.Name, uploadedObject.ObjectKey, round));
            }
            try
            {
                await MonitorWorkersWithListingsAsync(access, bucket, uploadedObjects.Select(uploadedObject => uploadedObject.ObjectKey).ToList(), settings, round, workers).ConfigureAwait(false);

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
            foreach (var stressObject in stressObjects)
            {
                try
                {
                    if (stressObject.Uploaded)
                    {
                        await objectService.DeleteObjectAsync(bucket, stressObject.ObjectKey).ConfigureAwait(false);
                        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: deleted uploaded object {bucket.Name}/{stressObject.ObjectKey}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: object cleanup failed non-fatally for {stressObject.ObjectKey}: {ex.Message}");
                }

                try
                {
                    if (File.Exists(stressObject.TempFilePath))
                    {
                        File.Delete(stressObject.TempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: temp file cleanup failed non-fatally for {stressObject.TempFilePath}: {ex.Message}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: object I/O stress failed non-fatally: {ex.Message}");
    }
}

static async Task<List<StressObjectPlan>> CreateStressObjectsAsync(Settings settings, int round)
{
    var stressObjects = Enumerable.Range(1, settings.ParallelUploadObjects)
        .Select(index => new StressObjectPlan
        {
            Index = index,
            FileSizeBytes = PickRandomFileSizeBytes(settings),
            ObjectKey = $"uplink-repro/{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{index}-{Guid.NewGuid():N}.bin",
            TempFilePath = Path.Combine(Path.GetTempPath(), $"uplink-repro-{Guid.NewGuid():N}.bin")
        })
        .ToList();

    await Task.WhenAll(stressObjects.Select(async stressObject =>
    {
        await CreateRandomFileAsync(stressObject.TempFilePath, stressObject.FileSizeBytes).ConfigureAwait(false);
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: generated random file {stressObject.TempFilePath} ({stressObject.FileSizeBytes / (1024d * 1024d):F1} MiB) for object #{stressObject.Index}");
    })).ConfigureAwait(false);

    return stressObjects;
}

static async Task UploadStressObjectAsync(ObjectService objectService, Bucket bucket, StressObjectPlan stressObject, int round)
{
    try
    {
        await using var uploadStream = new FileStream(stressObject.TempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
        using var uploadOperation = await objectService.UploadObjectAsync(bucket, stressObject.ObjectKey, new UploadOptions(), uploadStream, false).ConfigureAwait(false);
        await RequireStarted(uploadOperation.StartUploadAsync(), "Upload").ConfigureAwait(false);

        if (!uploadOperation.Completed || uploadOperation.Failed)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: upload failed non-fatally for object #{stressObject.Index}: {uploadOperation.ErrorMessage}");
            return;
        }

        stressObject.Uploaded = true;
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: uploaded {uploadOperation.BytesSent} bytes to {bucket.Name}/{stressObject.ObjectKey}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: upload failed non-fatally for object #{stressObject.Index}: {ex.Message}");
    }
}

static async Task UploadPendingCrashArtifactsAsync(Settings settings, string phase)
{
    await UploadPendingCrashArtifactsWithRetriesAsync(settings, phase, 1, 0).ConfigureAwait(false);
}

static async Task UploadPendingCrashArtifactsWithRetriesAsync(Settings settings, string phase, int attempts, int delayBetweenAttemptsMs)
{
    for (var attempt = 1; attempt <= attempts; attempt++)
    {
        try
        {
            var pendingFiles = GetPendingCrashArtifactFiles(settings).ToList();
            await AnalyzePendingCrashArtifactsWithGdbAsync(settings, phase, pendingFiles).ConfigureAwait(false);
            pendingFiles = GetPendingCrashArtifactFiles(settings).ToList();
            var createdBundles = await CrashArtifactBundler.CreateCrashAnalysisBundlesAsync(settings, phase, pendingFiles).ConfigureAwait(false);
            if (createdBundles.Count > 0)
            {
                pendingFiles = GetPendingCrashArtifactFiles(settings).ToList();
            }

            if (pendingFiles.Count == 0)
            {
                if (attempt < attempts && delayBetweenAttemptsMs > 0)
                {
                    await Task.Delay(delayBetweenAttemptsMs).ConfigureAwait(false);
                }

                continue;
            }

            var attemptSuffix = attempts > 1 ? $" (attempt {attempt}/{attempts})" : string.Empty;
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Found {pendingFiles.Count} pending crash artifact(s) during {phase}{attemptSuffix}; attempting upload.");

            using var access = CreateAccess(settings);
            using var bucket = await GetBucketAsync(access, settings.CrashArtifactBucketName).ConfigureAwait(false);

            var objectService = new ObjectService(access);
            foreach (var filePath in pendingFiles)
            {
                await TryUploadCrashArtifactAsync(objectService, bucket, settings, filePath, phase).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash artifact scan/upload failed non-fatally during {phase}: {ex.Message}");
        }

        if (attempt < attempts && delayBetweenAttemptsMs > 0)
        {
            await Task.Delay(delayBetweenAttemptsMs).ConfigureAwait(false);
        }
    }
}

static async Task AnalyzePendingCrashArtifactsWithGdbAsync(Settings settings, string phase, IReadOnlyCollection<string> pendingFiles)
{
    foreach (var filePath in pendingFiles.Where(IsGdbCandidateCrashArtifact).OrderBy(path => path, StringComparer.Ordinal))
    {
        await TryAnalyzeCrashArtifactWithGdbAsync(settings, phase, filePath).ConfigureAwait(false);
    }
}

static bool IsGdbCandidateCrashArtifact(string path)
{
    var fileName = Path.GetFileName(path);
    return fileName.StartsWith("coredump.", StringComparison.Ordinal)
        || (fileName.StartsWith("core", StringComparison.Ordinal) && !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        || fileName.EndsWith(".dmp", StringComparison.OrdinalIgnoreCase);
}

static async Task TryAnalyzeCrashArtifactWithGdbAsync(Settings settings, string phase, string dumpPath)
{
    var reportPath = Path.Combine(settings.CrashArtifactDirectory, $"{Path.GetFileName(dumpPath)}.gdb-report.txt");
    if (File.Exists(reportPath))
    {
        return;
    }

    var gdbPath = FindExecutableOnPath("gdb");
    if (string.IsNullOrWhiteSpace(gdbPath))
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Skipping gdb analysis for {dumpPath} because gdb is not installed.");
        return;
    }

    var executablePath = ResolveDotnetHostPath();
    if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Skipping gdb analysis for {dumpPath} because the dotnet host path could not be resolved.");
        return;
    }

    try
    {
        await WaitForCrashArtifactToStabilizeAsync(dumpPath).ConfigureAwait(false);

        var startInfo = new ProcessStartInfo
        {
            FileName = gdbPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = settings.CrashArtifactDirectory
        };
        startInfo.ArgumentList.Add("-batch");
        startInfo.ArgumentList.Add("-q");
        startInfo.ArgumentList.Add("-nx");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("set pagination off");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("set confirm off");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("set print thread-events off");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("info sharedlibrary");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("thread apply all bt full");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("frame 0");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("info symbol $pc");
        startInfo.ArgumentList.Add("-ex");
        startInfo.ArgumentList.Add("info line *$pc");
        startInfo.ArgumentList.Add(executablePath);
        startInfo.ArgumentList.Add(dumpPath);

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] gdb analysis failed non-fatally for {dumpPath}: process could not be started.");
            return;
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().ConfigureAwait(false);

        var output = await outputTask.ConfigureAwait(false);
        var error = await errorTask.ConfigureAwait(false);
        var storjSummary = ExtractStorjBacktraceSummary(output, error);

        var report = new StringBuilder()
            .AppendLine("GDB crash analysis report")
            .AppendLine($"GeneratedAtUtc: {DateTimeOffset.UtcNow:O}")
            .AppendLine($"Phase: {phase}")
            .AppendLine($"DumpPath: {dumpPath}")
            .AppendLine($"ExecutablePath: {executablePath}")
            .AppendLine($"GdbPath: {gdbPath}")
            .AppendLine($"ExitCode: {process.ExitCode}")
            .AppendLine()
            .AppendLine("storj_uplink.so summary:")
            .AppendLine(storjSummary)
            .AppendLine()
            .AppendLine("gdb stdout:")
            .AppendLine(string.IsNullOrWhiteSpace(output) ? "<empty>" : output)
            .AppendLine()
            .AppendLine("gdb stderr:")
            .AppendLine(string.IsNullOrWhiteSpace(error) ? "<empty>" : error)
            .ToString();

        await File.WriteAllTextAsync(reportPath, report).ConfigureAwait(false);
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] gdb analysis for {dumpPath}: {storjSummary.Replace(Environment.NewLine, " | ")}");
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Wrote gdb crash analysis report to {reportPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] gdb analysis failed non-fatally for {dumpPath}: {ex.Message}");
    }
}

static async Task WaitForCrashArtifactToStabilizeAsync(string filePath)
{
    if (!File.Exists(filePath))
    {
        return;
    }

    (long Length, DateTime LastWriteTimeUtc)? previous = null;
    for (var attempt = 0; attempt < 8; attempt++)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            return;
        }

        var current = (fileInfo.Length, fileInfo.LastWriteTimeUtc);
        if (previous == current)
        {
            return;
        }

        previous = current;
        await Task.Delay(500).ConfigureAwait(false);
    }
}

static string ExtractStorjBacktraceSummary(string output, string error)
{
    var combined = string.Join(Environment.NewLine, new[] { output, error }.Where(text => !string.IsNullOrWhiteSpace(text)));
    var storjLines = combined
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(line => line.Contains("storj_uplink.so", StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.Ordinal)
        .Take(8)
        .ToList();

    return storjLines.Count > 0
        ? string.Join(Environment.NewLine, storjLines)
        : "No storj_uplink.so frame was identified by gdb.";
}

static string? FindExecutableOnPath(string executableName)
{
    var pathValue = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrWhiteSpace(pathValue))
    {
        return null;
    }

    foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        try
        {
            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        catch
        {
        }
    }

    return null;
}

static string? ResolveDotnetHostPath()
{
    try
    {
        const string procSelfExe = "/proc/self/exe";
        if (File.Exists(procSelfExe))
        {
            var fileInfo = new FileInfo(procSelfExe);
            if (!string.IsNullOrWhiteSpace(fileInfo.LinkTarget))
            {
                var resolvedPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(procSelfExe)!, fileInfo.LinkTarget));
                if (File.Exists(resolvedPath))
                {
                    return resolvedPath;
                }
            }
        }
    }
    catch
    {
    }

    return Environment.ProcessPath;
}

static async Task<int> RunSupervisorAsync(Settings settings, IReadOnlyList<string> originalArgs)
{
    var startedAtUtc = DateTimeOffset.UtcNow;
    await UploadPendingCrashArtifactsAsync(settings, "startup").ConfigureAwait(false);

    for (var round = 1; round <= settings.Rounds; round++)
    {
        await UploadPendingCrashArtifactsAsync(settings, $"before-round-{round}").ConfigureAwait(false);

        using var child = StartSupervisorChild(settings, originalArgs, round);
        await child.WaitForExitAsync().ConfigureAwait(false);
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Supervisor: child PID={child.Id} for round {round}/{settings.Rounds} exited with code {child.ExitCode}");

        var uploadAttempts = child.ExitCode == 0 ? 1 : 5;
        var uploadDelayMs = child.ExitCode == 0 ? 0 : 1000;
        await UploadPendingCrashArtifactsWithRetriesAsync(settings, $"after-round-{round}", uploadAttempts, uploadDelayMs).ConfigureAwait(false);
        if (child.ExitCode != 0)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Supervisor: stopping after round {round} because the child failed or crashed.");
            return child.ExitCode;
        }
    }

    Console.WriteLine($"Completed without a native crash after {(DateTimeOffset.UtcNow - startedAtUtc).TotalSeconds:F1}s.");
    Console.WriteLine("If the old package did not segfault yet, rerun with larger --rounds/--churn values or enable more object I/O stress.");
    return 0;
}

static async Task<int> RunSupervisorChildAsync(Settings settings)
{
    var round = settings.ChildRoundNumber ?? 1;
    await ExecuteRoundAsync(settings, round, settings.Rounds).ConfigureAwait(false);
    return 0;
}

static async Task ExecuteRoundAsync(Settings settings, int round, int totalRounds)
{
    WriteRoundState(settings, round);
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}/{totalRounds}: creating primary access");

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
                    await TryListBucketsAsync(throwaway, "throwaway", round, churn).ConfigureAwait(false);
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
        await TryExerciseObjectIoAsync(primary, settings, round).ConfigureAwait(false);
    }

    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: using primary access after churn");
    var serialized = SerializeRepeatedly(primary, settings.SerializeRepeats);
    Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: serialize() returned {serialized.Length} characters");

    if (settings.ExerciseBucketListing)
    {
        await TryListBucketsAsync(primary, "primary", round, settings.ChurnPerRound).ConfigureAwait(false);
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    TryClearRoundState(settings);
}

static IEnumerable<string> GetPendingCrashArtifactFiles(Settings settings)
{
    var seenPaths = new HashSet<string>(StringComparer.Ordinal);
    foreach (var searchSpec in GetCrashArtifactSearchSpecs(settings))
    {
        if (!Directory.Exists(searchSpec.DirectoryPath))
        {
            continue;
        }

        IEnumerable<string> candidates;
        try
        {
            candidates = Directory.EnumerateFiles(searchSpec.DirectoryPath, searchSpec.SearchPattern, searchSpec.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash artifact scan skipped {searchSpec.DirectoryPath} ({searchSpec.SearchPattern}) non-fatally: {ex.Message}");
            continue;
        }

        foreach (var filePath in candidates)
        {
            var fullPath = Path.GetFullPath(filePath);
            if (!seenPaths.Add(fullPath))
            {
                continue;
            }

            if (IsUploadedCrashArtifact(settings, fullPath))
            {
                continue;
            }

            yield return fullPath;
        }
    }
}

static async Task TryUploadCrashArtifactAsync(ObjectService objectService, Bucket bucket, Settings settings, string filePath, string phase)
{
    var objectKey = BuildCrashArtifactObjectKey(settings, filePath);

    try
    {
        await using var artifactStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1024 * 1024, useAsync: true);
        using var uploadOperation = await objectService.UploadObjectAsync(bucket, objectKey, new UploadOptions(), artifactStream, false).ConfigureAwait(false);
        await RequireStarted(uploadOperation.StartUploadAsync(), "Crash artifact upload").ConfigureAwait(false);

        if (!uploadOperation.Completed || uploadOperation.Failed)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash artifact upload failed non-fatally during {phase} for {filePath}: {uploadOperation.ErrorMessage}");
            return;
        }

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Uploaded crash artifact {filePath} to {bucket.Name}/{objectKey}");
        MoveCrashArtifactToUploadedDirectory(settings, filePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash artifact upload failed non-fatally during {phase} for {filePath}: {ex.Message}");
    }
}

static string BuildCrashArtifactObjectKey(Settings settings, string filePath)
{
    var relativePath = IsPathUnderDirectory(filePath, settings.CrashArtifactDirectory)
        ? Path.GetRelativePath(settings.CrashArtifactDirectory, filePath).Replace(Path.DirectorySeparatorChar, '/')
        : $"external/{Path.GetFileName(filePath)}";
    var host = Environment.MachineName;
    return $"{settings.CrashArtifactPrefix.TrimEnd('/')}/{host}/{relativePath}";
}

static void MoveCrashArtifactToUploadedDirectory(Settings settings, string filePath)
{
    try
    {
        var relativePath = IsPathUnderDirectory(filePath, settings.CrashArtifactDirectory)
            ? Path.GetRelativePath(settings.CrashArtifactDirectory, filePath)
            : Path.Combine("external", Path.GetFileName(filePath));
        var destinationPath = Path.Combine(settings.CrashArtifactDirectory, "uploaded", relativePath);
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        var candidatePath = destinationPath;
        var suffix = 1;
        while (File.Exists(candidatePath))
        {
            candidatePath = Path.Combine(
                Path.GetDirectoryName(destinationPath) ?? settings.CrashArtifactDirectory,
                $"{Path.GetFileNameWithoutExtension(destinationPath)}-{suffix}{Path.GetExtension(destinationPath)}");
            suffix++;
        }

        File.Move(filePath, candidatePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash artifact post-upload move failed non-fatally for {filePath}: {ex.Message}");
    }
}

static void WriteRoundState(Settings settings, int round)
{
    try
    {
        var statePath = Path.Combine(settings.CrashArtifactDirectory, "active-round.json");
        if (File.Exists(statePath))
        {
            var stalePath = Path.Combine(
                settings.CrashArtifactDirectory,
                $"stale-active-round-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.json");
            File.Move(statePath, stalePath, overwrite: false);
        }

        var state = new CrashRoundState
        {
            Round = round,
            StartedAtUtc = DateTimeOffset.UtcNow,
            ProcessId = Environment.ProcessId,
            MachineName = Environment.MachineName,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            OsDescription = RuntimeInformation.OSDescription,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            CommandLine = Environment.CommandLine,
            CrashDiagnostics = CaptureCrashDiagnosticsSnapshot(settings)
        };

        File.WriteAllText(statePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Failed to write crash round state non-fatally: {ex.Message}");
    }
}

static void TryClearRoundState(Settings settings)
{
    try
    {
        var statePath = Path.Combine(settings.CrashArtifactDirectory, "active-round.json");
        if (File.Exists(statePath))
        {
            File.Delete(statePath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Failed to clear crash round state non-fatally: {ex.Message}");
    }
}

static bool IsCrashDumpCollectionConfigured(Settings settings)
{
    var enabled = Environment.GetEnvironmentVariable("DOTNET_DbgEnableMiniDump")
        ?? Environment.GetEnvironmentVariable("COMPlus_DbgEnableMiniDump");
    var dumpName = Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName")
        ?? Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpName");

    return string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(dumpName);
}

static void EnsureCrashDiagnosticsEnvironment(Settings settings)
{
    var dumpName = FirstNonEmptyEnvironment("DOTNET_DbgMiniDumpName", "COMPlus_DbgMiniDumpName")
        ?? Path.Combine(settings.CrashArtifactDirectory, "coredump.%p.%e.%h.%t.dmp");
    var dumpType = FirstNonEmptyEnvironment("DOTNET_DbgMiniDumpType", "COMPlus_DbgMiniDumpType") ?? "4";
    var dumpLogPath = Environment.GetEnvironmentVariable("DOTNET_CreateDumpLogToFile")
        ?? Path.Combine(settings.CrashArtifactDirectory, "createdump.%p.%e.%h.%t.log");

    SetEnvironmentIfMissing("DOTNET_DbgEnableMiniDump", "1");
    SetEnvironmentIfMissing("COMPlus_DbgEnableMiniDump", Environment.GetEnvironmentVariable("DOTNET_DbgEnableMiniDump") ?? "1");
    SetEnvironmentIfMissing("DOTNET_DbgMiniDumpName", dumpName);
    SetEnvironmentIfMissing("COMPlus_DbgMiniDumpName", Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName") ?? dumpName);
    SetEnvironmentIfMissing("DOTNET_DbgMiniDumpType", dumpType);
    SetEnvironmentIfMissing("COMPlus_DbgMiniDumpType", Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpType") ?? dumpType);
    SetEnvironmentIfMissing("DOTNET_EnableCrashReport", "1");
    SetEnvironmentIfMissing("DOTNET_CreateDumpDiagnostics", "1");
    SetEnvironmentIfMissing("DOTNET_CreateDumpVerboseDiagnostics", "1");
    SetEnvironmentIfMissing("DOTNET_CreateDumpLogToFile", dumpLogPath);
}

static void SetEnvironmentIfMissing(string environmentName, string value)
{
    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentName)))
    {
        return;
    }

    Environment.SetEnvironmentVariable(environmentName, value);
}

static string? FirstNonEmptyEnvironment(params string[] environmentNames)
{
    foreach (var environmentName in environmentNames)
    {
        var value = Environment.GetEnvironmentVariable(environmentName);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    return null;
}

static void ApplyCrashDiagnosticsEnvironment(ProcessStartInfo startInfo, Settings settings)
{
    startInfo.WorkingDirectory = settings.CrashArtifactDirectory;
    CopyEnvironment(startInfo, "DOTNET_DbgEnableMiniDump", "1");
    CopyEnvironment(startInfo, "COMPlus_DbgEnableMiniDump", Environment.GetEnvironmentVariable("COMPlus_DbgEnableMiniDump") ?? Environment.GetEnvironmentVariable("DOTNET_DbgEnableMiniDump") ?? "1");
    CopyEnvironment(startInfo, "DOTNET_DbgMiniDumpName", Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName") ?? Path.Combine(settings.CrashArtifactDirectory, "coredump.%p.%e.%h.%t.dmp"));
    CopyEnvironment(startInfo, "COMPlus_DbgMiniDumpName", Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpName") ?? Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName") ?? Path.Combine(settings.CrashArtifactDirectory, "coredump.%p.%e.%h.%t.dmp"));
    CopyEnvironment(startInfo, "DOTNET_DbgMiniDumpType", Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpType") ?? "4");
    CopyEnvironment(startInfo, "COMPlus_DbgMiniDumpType", Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpType") ?? Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpType") ?? "4");
    CopyEnvironment(startInfo, "DOTNET_EnableCrashReport", Environment.GetEnvironmentVariable("DOTNET_EnableCrashReport") ?? "1");
    CopyEnvironment(startInfo, "DOTNET_CreateDumpDiagnostics", Environment.GetEnvironmentVariable("DOTNET_CreateDumpDiagnostics") ?? "1");
    CopyEnvironment(startInfo, "DOTNET_CreateDumpVerboseDiagnostics", Environment.GetEnvironmentVariable("DOTNET_CreateDumpVerboseDiagnostics") ?? "1");
    CopyEnvironment(startInfo, "DOTNET_CreateDumpLogToFile", Environment.GetEnvironmentVariable("DOTNET_CreateDumpLogToFile") ?? Path.Combine(settings.CrashArtifactDirectory, "createdump.%p.%e.%h.%t.log"));
}

static void CopyEnvironment(ProcessStartInfo startInfo, string environmentName, string fallbackValue)
{
    startInfo.Environment[environmentName] = Environment.GetEnvironmentVariable(environmentName) ?? fallbackValue;
}

static void TryRaiseNativeCoreDumpLimit()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return;
    }

    try
    {
        if (NativeMethods.getrlimit(NativeMethods.RlimitCoreResource, out var limit) != 0)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Could not query RLIMIT_CORE non-fatally: errno={Marshal.GetLastWin32Error()}");
            return;
        }

        if (limit.Current == limit.Maximum || limit.Maximum == 0)
        {
            return;
        }

        var updatedLimit = new RLimit(limit.Maximum, limit.Maximum);
        if (NativeMethods.setrlimit(NativeMethods.RlimitCoreResource, ref updatedLimit) != 0)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Could not raise RLIMIT_CORE non-fatally: errno={Marshal.GetLastWin32Error()}, current={FormatRLimit(limit.Current)}, max={FormatRLimit(limit.Maximum)}");
            return;
        }

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Raised RLIMIT_CORE from {FormatRLimit(limit.Current)} to {FormatRLimit(updatedLimit.Current)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Could not raise RLIMIT_CORE non-fatally: {ex.Message}");
    }
}

static void LogCrashDiagnosticsConfiguration(Settings settings)
{
    var snapshot = CaptureCrashDiagnosticsSnapshot(settings);
    Console.WriteLine(
        $"[{DateTimeOffset.UtcNow:O}] Crash diagnostics configured: " +
        $"DumpEnabled={snapshot.DotNetDbgEnableMiniDump ?? snapshot.ComPlusDbgEnableMiniDump ?? "<unset>"}, " +
        $"DumpName={snapshot.DotNetDbgMiniDumpName ?? snapshot.ComPlusDbgMiniDumpName ?? "<unset>"}, " +
        $"DumpType={snapshot.DotNetDbgMiniDumpType ?? snapshot.ComPlusDbgMiniDumpType ?? "<unset>"}, " +
        $"CrashReport={snapshot.DotNetEnableCrashReport ?? "<unset>"}, " +
        $"CreateDumpDiagnostics={snapshot.DotNetCreateDumpDiagnostics ?? "<unset>"}, " +
        $"CreateDumpVerboseDiagnostics={snapshot.DotNetCreateDumpVerboseDiagnostics ?? "<unset>"}, " +
        $"CreateDumpLog={snapshot.DotNetCreateDumpLogToFile ?? "<unset>"}, " +
        $"CorePattern={snapshot.LinuxCorePattern ?? "<unavailable>"}, " +
        $"CoreUsesPid={snapshot.LinuxCoreUsesPid ?? "<unavailable>"}, " +
        $"CoreLimit={snapshot.CoreLimitCurrent ?? "<unavailable>"}/{snapshot.CoreLimitMaximum ?? "<unavailable>"}");
}

static CrashDiagnosticsSnapshot CaptureCrashDiagnosticsSnapshot(Settings settings)
{
    var processPath = Environment.ProcessPath;
    var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
    string? coreLimitCurrent = null;
    string? coreLimitMaximum = null;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && NativeMethods.getrlimit(NativeMethods.RlimitCoreResource, out var limit) == 0)
    {
        coreLimitCurrent = FormatRLimit(limit.Current);
        coreLimitMaximum = FormatRLimit(limit.Maximum);
    }

    return new CrashDiagnosticsSnapshot
    {
        DotNetDbgEnableMiniDump = Environment.GetEnvironmentVariable("DOTNET_DbgEnableMiniDump"),
        ComPlusDbgEnableMiniDump = Environment.GetEnvironmentVariable("COMPlus_DbgEnableMiniDump"),
        DotNetDbgMiniDumpName = Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName"),
        ComPlusDbgMiniDumpName = Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpName"),
        DotNetDbgMiniDumpType = Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpType"),
        ComPlusDbgMiniDumpType = Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpType"),
        DotNetEnableCrashReport = Environment.GetEnvironmentVariable("DOTNET_EnableCrashReport"),
        DotNetCreateDumpDiagnostics = Environment.GetEnvironmentVariable("DOTNET_CreateDumpDiagnostics"),
        DotNetCreateDumpVerboseDiagnostics = Environment.GetEnvironmentVariable("DOTNET_CreateDumpVerboseDiagnostics"),
        DotNetCreateDumpLogToFile = Environment.GetEnvironmentVariable("DOTNET_CreateDumpLogToFile"),
        ProcessPath = processPath ?? string.Empty,
        EntryAssemblyPath = entryAssemblyPath ?? string.Empty,
        CrashArtifactDirectory = settings.CrashArtifactDirectory,
        LinuxCorePattern = TryReadTrimmedFile("/proc/sys/kernel/core_pattern"),
        LinuxCoreUsesPid = TryReadTrimmedFile("/proc/sys/kernel/core_uses_pid"),
        LinuxCoreDumpFilter = TryReadTrimmedFile("/proc/self/coredump_filter"),
        CoreLimitCurrent = coreLimitCurrent,
        CoreLimitMaximum = coreLimitMaximum
    };
}

static string? TryReadTrimmedFile(string path)
{
    try
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return File.ReadAllText(path).Trim();
    }
    catch
    {
        return null;
    }
}

static IEnumerable<CrashArtifactSearchSpec> GetCrashArtifactSearchSpecs(Settings settings)
{
    yield return new CrashArtifactSearchSpec(settings.CrashArtifactDirectory, "*", true);

    foreach (var configuredPath in GetConfiguredCrashArtifactPaths(settings))
    {
        var directoryPath = Path.GetDirectoryName(configuredPath);
        var fileName = Path.GetFileName(configuredPath);
        if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(fileName))
        {
            continue;
        }

        yield return new CrashArtifactSearchSpec(directoryPath, ConvertCrashPatternToSearchPattern(fileName), false);
    }

    var corePattern = TryReadTrimmedFile("/proc/sys/kernel/core_pattern");
    if (!string.IsNullOrWhiteSpace(corePattern) && !corePattern.StartsWith('|') && Path.IsPathRooted(corePattern))
    {
        var coreDirectory = Path.GetDirectoryName(corePattern);
        var coreFileName = Path.GetFileName(corePattern);
        if (!string.IsNullOrWhiteSpace(coreDirectory) && !string.IsNullOrWhiteSpace(coreFileName))
        {
            yield return new CrashArtifactSearchSpec(coreDirectory, ConvertCrashPatternToSearchPattern(coreFileName), false);
        }
    }
}

static IEnumerable<string> GetConfiguredCrashArtifactPaths(Settings settings)
{
    var dumpName = Environment.GetEnvironmentVariable("DOTNET_DbgMiniDumpName")
        ?? Environment.GetEnvironmentVariable("COMPlus_DbgMiniDumpName");
    if (!string.IsNullOrWhiteSpace(dumpName) && Path.IsPathRooted(dumpName))
    {
        yield return dumpName;
        yield return $"{dumpName}.crashreport.json";
    }

    var createDumpLog = Environment.GetEnvironmentVariable("DOTNET_CreateDumpLogToFile");
    if (!string.IsNullOrWhiteSpace(createDumpLog) && Path.IsPathRooted(createDumpLog))
    {
        yield return createDumpLog;
    }
}

static string ConvertCrashPatternToSearchPattern(string fileName)
{
    var pattern = Regex.Replace(fileName, "%.", "*");
    return string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;
}

static bool IsUploadedCrashArtifact(Settings settings, string fullPath)
{
    var uploadedDirectory = Path.GetFullPath(Path.Combine(settings.CrashArtifactDirectory, "uploaded"));
    return fullPath.StartsWith(uploadedDirectory, StringComparison.Ordinal);
}

static bool IsPathUnderDirectory(string filePath, string directoryPath)
{
    var fullFilePath = Path.GetFullPath(filePath);
    var fullDirectoryPath = Path.GetFullPath(directoryPath)
        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + Path.DirectorySeparatorChar;
    return fullFilePath.StartsWith(fullDirectoryPath, StringComparison.Ordinal);
}

static string FormatRLimit(ulong value)
{
    return value == ulong.MaxValue ? "unlimited" : value.ToString();
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
        var downloadTask = RequireStarted(downloadOperation.StartDownloadAsync(), "Download");
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
        var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine the current process path. Run the repro via dotnet run or a published executable so worker downloads can be spawned.");
        var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            WorkingDirectory = settings.CrashArtifactDirectory
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
        ApplyCrashDiagnosticsEnvironment(startInfo, settings);

        Process worker;
        try
        {
            worker = Process.Start(startInfo) ?? throw new InvalidOperationException("Process.Start returned null for the download worker.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start the download worker process for {bucketName}/{objectKey}: {ex.Message}", ex);
        }

        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}: started download worker PID={worker.Id} for {bucketName}/{objectKey}");
        workers.Add(worker);
    }

    return workers;
}

static Process StartSupervisorChild(Settings settings, IReadOnlyList<string> originalArgs, int round)
{
    var startInfo = CreateCurrentProcessStartInfo(settings);
    foreach (var arg in originalArgs)
    {
        if (string.Equals(arg, "--supervisor-child", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        startInfo.ArgumentList.Add(arg);
    }

    startInfo.ArgumentList.Add("--supervisor-child");
    startInfo.ArgumentList.Add("--child-round");
    startInfo.ArgumentList.Add(round.ToString());

    try
    {
        var child = Process.Start(startInfo) ?? throw new InvalidOperationException("Process.Start returned null for the supervised round child.");
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Supervisor: started child PID={child.Id} for round {round}/{settings.Rounds}");
        return child;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to start the supervised round child for round {round}: {ex.Message}", ex);
    }
}

static ProcessStartInfo CreateCurrentProcessStartInfo(Settings settings)
{
    var processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine the current process path. Run the repro via dotnet run or a published executable so child processes can be spawned.");
    var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
    var startInfo = new ProcessStartInfo
    {
        UseShellExecute = false,
        WorkingDirectory = settings.CrashArtifactDirectory
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

    ApplyCrashDiagnosticsEnvironment(startInfo, settings);
    return startInfo;
}

static async Task MonitorWorkersWithListingsAsync(Access access, Bucket bucket, IReadOnlyCollection<string> objectKeys, Settings settings, int round, List<Process> workers)
{
    var objectService = new ObjectService(access);

    while (workers.Any(worker => !worker.HasExited))
    {
        await TryListBucketsAsync(access, "object-io-monitor", round, settings.ChurnPerRound).ConfigureAwait(false);
        await TryListObjectsForKeysAsync(objectService, bucket, objectKeys, "object-io-monitor", round).ConfigureAwait(false);

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
    await TryListObjectsForKeysAsync(objectService, bucket, new[] { objectKey }, label, round).ConfigureAwait(false);
}

static async Task TryListObjectsForKeysAsync(ObjectService objectService, Bucket bucket, IReadOnlyCollection<string> objectKeys, string label, int round)
{
    try
    {
        var objectList = await objectService.ListObjectsAsync(bucket, new ListObjectsOptions()).ConfigureAwait(false);
        var targetKeys = objectKeys.Count == 0
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(objectKeys, StringComparer.Ordinal);
        var foundTargets = objectList.Items.Count(item => targetKeys.Contains(item.Key));
        Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Round {round}, {label}: listed {objectList.Items.Count} objects in {bucket.Name}; foundTargets={foundTargets}/{targetKeys.Count}");
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
    // Deliberately create extra managed allocations so finalizers/GC run more often while native handles are churning.
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

static Task RequireStarted(Task? task, string operationName)
{
    return task ?? throw new InvalidOperationException($"{operationName} did not start because the operation was already completed, failed, or cancelled.");
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
    public int? ChildRoundNumber { get; private init; }
    public string CrashArtifactBucketName { get; private init; } = "s-drive";
    public string CrashArtifactDirectory { get; private init; } = Path.Combine(Path.GetTempPath(), "uplink-repro-crash-artifacts");
    public string CrashArtifactPrefix { get; private init; } = "uplink-repro/crash-artifacts";
    public int ParallelUploadObjects { get; private init; } = 8;
    public int Rounds { get; private init; } = 10;
    public int ChurnPerRound { get; private init; } = 100;
    public int StatusEvery { get; private init; } = 25;
    public int BucketListEvery { get; private init; } = 25;
    public int SerializeRepeats { get; private init; } = 1;
    public int DisposeBatchSize { get; private init; } = 1;
    public int ObjectIoEveryRounds { get; private init; } = 1;
    public int MinFileSizeMb { get; private init; } = 1;
    public int MaxFileSizeMb { get; private init; } = 4;
    public int ParallelDownloadProcesses { get; private init; } = 8;
    public int ObjectIoWaitMs { get; private init; } = 2000;
    public int ObjectIoListingDelayMs { get; private init; } = 250;
    public bool ExerciseBucketListing { get; private init; }
    public bool ReparseAfterSerialize { get; private init; }
    public bool ExerciseObjectIo { get; private init; }
    public bool WorkerDownloadMode { get; private init; }
    public bool SupervisorChildMode { get; private init; }
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
                case "--supervisor-child":
                    flags.Add("supervisor-child");
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
                case "--parallel-upload-objects":
                case "--min-file-size-mb":
                case "--max-file-size-mb":
                case "--parallel-download-processes":
                case "--object-io-wait-ms":
                case "--object-io-listing-delay-ms":
                case "--worker-bucket":
                case "--worker-object-key":
                case "--worker-label":
                case "--child-round":
                case "--crash-artifact-bucket":
                case "--crash-artifact-dir":
                case "--crash-artifact-prefix":
                    if (i + 1 >= args.Length)
                    {
                        throw new ArgumentException($"Missing value for {arg}.");
                    }

                    values[arg] = args[++i];
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {arg}. Use --help to see the available options.");
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
            ChildRoundNumber = ParseOptionalPositiveInt(values, "--child-round"),
            CrashArtifactBucketName = FirstNonEmpty(values, "--crash-artifact-bucket", "UPLINK_REPRO_CRASH_ARTIFACT_BUCKET") ?? "s-drive",
            CrashArtifactDirectory = FirstNonEmpty(values, "--crash-artifact-dir", "UPLINK_REPRO_CRASH_ARTIFACT_DIR") ?? Path.Combine(Path.GetTempPath(), "uplink-repro-crash-artifacts"),
            CrashArtifactPrefix = FirstNonEmpty(values, "--crash-artifact-prefix", "UPLINK_REPRO_CRASH_ARTIFACT_PREFIX") ?? "uplink-repro/crash-artifacts",
            Rounds = ParsePositiveInt(values, "--rounds", "UPLINK_REPRO_ROUNDS", 10),
            ChurnPerRound = ParsePositiveInt(values, "--churn", "UPLINK_REPRO_CHURN", 100),
            StatusEvery = ParsePositiveInt(values, "--status-every", "UPLINK_REPRO_STATUS_EVERY", 25),
            BucketListEvery = ParsePositiveInt(values, "--bucket-list-every", "UPLINK_REPRO_BUCKET_LIST_EVERY", 25),
            SerializeRepeats = ParsePositiveInt(values, "--serialize-repeats", "UPLINK_REPRO_SERIALIZE_REPEATS", 1),
            DisposeBatchSize = ParsePositiveInt(values, "--dispose-batch-size", "UPLINK_REPRO_DISPOSE_BATCH_SIZE", 1),
            ObjectIoEveryRounds = ParsePositiveInt(values, "--object-io-every-rounds", "UPLINK_REPRO_OBJECT_IO_EVERY_ROUNDS", 1),
            ParallelUploadObjects = ParsePositiveInt(values, "--parallel-upload-objects", "UPLINK_REPRO_PARALLEL_UPLOAD_OBJECTS", 8),
            MinFileSizeMb = ParsePositiveInt(values, "--min-file-size-mb", "UPLINK_REPRO_MIN_FILE_SIZE_MB", 1),
            MaxFileSizeMb = ParsePositiveInt(values, "--max-file-size-mb", "UPLINK_REPRO_MAX_FILE_SIZE_MB", 4),
            ParallelDownloadProcesses = ParsePositiveInt(values, "--parallel-download-processes", "UPLINK_REPRO_PARALLEL_DOWNLOAD_PROCESSES", 8),
            ObjectIoWaitMs = ParsePositiveInt(values, "--object-io-wait-ms", "UPLINK_REPRO_OBJECT_IO_WAIT_MS", 2000),
            ObjectIoListingDelayMs = ParsePositiveInt(values, "--object-io-listing-delay-ms", "UPLINK_REPRO_OBJECT_IO_LISTING_DELAY_MS", 250),
            ExerciseBucketListing = flags.Contains("list-buckets") || ParseBool("UPLINK_REPRO_LIST_BUCKETS"),
            ReparseAfterSerialize = flags.Contains("reparse-after-serialize") || ParseBool("UPLINK_REPRO_REPARSE_AFTER_SERIALIZE"),
            ExerciseObjectIo = flags.Contains("object-io") || ParseBool("UPLINK_REPRO_OBJECT_IO"),
            WorkerDownloadMode = flags.Contains("worker-download"),
            SupervisorChildMode = flags.Contains("supervisor-child")
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

        if (settings.SupervisorChildMode && settings.ChildRoundNumber is null)
        {
            throw new ArgumentException("Supervisor child mode requires --child-round.");
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
  --object-io                         Generate random files, upload them in parallel, wait, then spawn parallel download workers while listings continue.
  --object-io-every-rounds <count>    Run the object I/O stress every N rounds. Default: 1.
  --parallel-upload-objects <n>       Number of objects uploaded in parallel during each object-I/O round. Default: 8.
  --min-file-size-mb <count>          Minimum random object size in MiB. Default: 1.
  --max-file-size-mb <count>          Maximum random object size in MiB. Default: 4.
  --parallel-download-processes <n>   Number of worker processes per uploaded object. Default: 8.
  --object-io-wait-ms <count>         Delay after upload before parallel downloads begin. Default: 2000.
  --object-io-listing-delay-ms <n>    Delay between listing/serialize passes during object I/O stress. Default: 250.
  --crash-artifact-bucket <name>      Bucket used for uploaded crash dumps, analysis bundles, and related files. Default: s-drive.
  --crash-artifact-dir <path>         Directory scanned before each round for dump/error files and generated analysis bundles. Default: /tmp/uplink-repro-crash-artifacts.
  --crash-artifact-prefix <prefix>    Storj object key prefix for uploaded crash artifacts and bundles. Default: uplink-repro/crash-artifacts.
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

    private static int? ParseOptionalPositiveInt(Dictionary<string, string?> values, string argumentName)
    {
        if (!values.TryGetValue(argumentName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
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

internal sealed class CrashRoundState
{
    public int Round { get; init; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public int ProcessId { get; init; }
    public string MachineName { get; init; } = string.Empty;
    public string FrameworkDescription { get; init; } = string.Empty;
    public string OsDescription { get; init; } = string.Empty;
    public string WorkingDirectory { get; init; } = string.Empty;
    public string CommandLine { get; init; } = string.Empty;
    public CrashDiagnosticsSnapshot CrashDiagnostics { get; init; } = new();
}

internal sealed class StressObjectPlan
{
    public int Index { get; init; }
    public long FileSizeBytes { get; init; }
    public string ObjectKey { get; init; } = string.Empty;
    public string TempFilePath { get; init; } = string.Empty;
    public bool Uploaded { get; set; }
}

internal sealed class CrashDiagnosticsSnapshot
{
    public string? DotNetDbgEnableMiniDump { get; init; }
    public string? ComPlusDbgEnableMiniDump { get; init; }
    public string? DotNetDbgMiniDumpName { get; init; }
    public string? ComPlusDbgMiniDumpName { get; init; }
    public string? DotNetDbgMiniDumpType { get; init; }
    public string? ComPlusDbgMiniDumpType { get; init; }
    public string? DotNetEnableCrashReport { get; init; }
    public string? DotNetCreateDumpDiagnostics { get; init; }
    public string? DotNetCreateDumpVerboseDiagnostics { get; init; }
    public string? DotNetCreateDumpLogToFile { get; init; }
    public string ProcessPath { get; init; } = string.Empty;
    public string EntryAssemblyPath { get; init; } = string.Empty;
    public string CrashArtifactDirectory { get; init; } = string.Empty;
    public string? LinuxCorePattern { get; init; }
    public string? LinuxCoreUsesPid { get; init; }
    public string? LinuxCoreDumpFilter { get; init; }
    public string? CoreLimitCurrent { get; init; }
    public string? CoreLimitMaximum { get; init; }
}

internal sealed record CrashArtifactSearchSpec(string DirectoryPath, string SearchPattern, bool Recursive);

[StructLayout(LayoutKind.Sequential)]
internal struct RLimit
{
    public RLimit(ulong current, ulong maximum)
    {
        Current = current;
        Maximum = maximum;
    }

    public ulong Current;
    public ulong Maximum;
}

internal static class NativeMethods
{
    // Linux uses RLIMIT_CORE resource 4; this interop is only used on Linux to raise the native core-dump limit.
    internal const int RlimitCoreResource = 4;

    [DllImport("libc", SetLastError = true)]
    internal static extern int getrlimit(int resource, out RLimit rlim);

    [DllImport("libc", SetLastError = true)]
    internal static extern int setrlimit(int resource, ref RLimit rlim);
}
