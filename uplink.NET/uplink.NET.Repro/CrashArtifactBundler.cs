using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class CrashArtifactBundler
{
    private const int StabilityChecksRequired = 2;
    private const int StabilityPollDelayMs = 750;
    private const int StabilityMaxAttempts = 8;

    public static async Task<IReadOnlyList<string>> CreateCrashAnalysisBundlesAsync(Settings settings, string phase, IReadOnlyCollection<string> pendingFiles, CancellationToken cancellationToken = default)
    {
        var triggerFiles = pendingFiles
            .Select(Path.GetFullPath)
            .Where(path => !IsUploadedPath(settings, path))
            .Where(path => !IsBundlePath(settings, path))
            .Where(IsCrashTriggerFile)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (triggerFiles.Count == 0)
        {
            return Array.Empty<string>();
        }

        await WaitForStableFilesAsync(triggerFiles, cancellationToken).ConfigureAwait(false);

        var fingerprint = await ComputeFingerprintAsync(triggerFiles, cancellationToken).ConfigureAwait(false);
        if (FindExistingBundleDirectory(settings, fingerprint) is { } existingBundleDirectory)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash bundle already exists for fingerprint {fingerprint}: {existingBundleDirectory}");
            return new[] { existingBundleDirectory };
        }

        var bundleDirectory = await CreateCrashAnalysisBundleAsync(settings, phase, triggerFiles, fingerprint, cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(bundleDirectory) ? Array.Empty<string>() : new[] { bundleDirectory };
    }

    public static async Task<string?> CreateCrashAnalysisBundleAsync(Settings settings, string phase, IReadOnlyCollection<string> sourceCrashFiles, CancellationToken cancellationToken = default)
    {
        var fingerprint = await ComputeFingerprintAsync(sourceCrashFiles, cancellationToken).ConfigureAwait(false);
        return await CreateCrashAnalysisBundleAsync(settings, phase, sourceCrashFiles, fingerprint, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string?> CreateCrashAnalysisBundleAsync(Settings settings, string phase, IReadOnlyCollection<string> sourceCrashFiles, string fingerprint, CancellationToken cancellationToken)
    {
        try
        {
            var representativeFile = sourceCrashFiles
                .Select(Path.GetFileName)
                .FirstOrDefault(fileName => !string.IsNullOrWhiteSpace(fileName));
            var inferredPid = TryExtractProcessId(representativeFile) ?? Environment.ProcessId;
            var bundleDirectory = Path.Combine(
                settings.CrashArtifactDirectory,
                $"bundle-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{inferredPid}");

            Directory.CreateDirectory(bundleDirectory);

            var manifest = new CrashAnalysisManifest
            {
                BundleCreatedAtUtc = DateTimeOffset.UtcNow,
                Phase = phase,
                Hostname = Environment.MachineName,
                ProcessId = inferredPid,
                CurrentProcessId = Environment.ProcessId,
                CommandLine = Environment.CommandLine,
                EnvironmentVersion = Environment.Version.ToString(),
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                OsDescription = RuntimeInformation.OSDescription,
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                DotnetInfo = await TryGetDotnetInfoAsync(cancellationToken).ConfigureAwait(false),
                CrashSourceFingerprint = fingerprint,
                SourceCrashFiles = sourceCrashFiles.OrderBy(path => path, StringComparer.Ordinal).ToList()
            };

            var copiedFiles = new List<CrashAnalysisCopiedFile>();

            foreach (var crashFile in EnumerateCrashCompanionFiles(settings, sourceCrashFiles))
            {
                var relativePath = Path.Combine("crash", Path.GetFileName(crashFile));
                var copiedFile = await TryCopyFileIntoBundleAsync(bundleDirectory, crashFile, relativePath, copiedFiles, cancellationToken).ConfigureAwait(false);
                if (copiedFile != null)
                {
                    copiedFiles.Add(copiedFile);
                }
            }

            foreach (var appFile in EnumerateAppFiles())
            {
                var relativePath = BuildBundleRelativePath(GetAppBaseDirectory(), appFile, "app");
                var copiedFile = await TryCopyFileIntoBundleAsync(bundleDirectory, appFile, relativePath, copiedFiles, cancellationToken).ConfigureAwait(false);
                if (copiedFile != null)
                {
                    copiedFiles.Add(copiedFile);
                }
            }

            foreach (var runtimeFile in EnumerateRuntimeFiles())
            {
                var relativePath = BuildRuntimeBundlePath(runtimeFile);
                var copiedFile = await TryCopyFileIntoBundleAsync(bundleDirectory, runtimeFile, relativePath, copiedFiles, cancellationToken).ConfigureAwait(false);
                if (copiedFile != null)
                {
                    copiedFiles.Add(copiedFile);
                }
            }

            var summaryPath = Path.Combine(bundleDirectory, "analysis-manifest.txt");
            manifest.CopiedFiles = copiedFiles;
            await File.WriteAllTextAsync(summaryPath, BuildManifestSummary(manifest), cancellationToken).ConfigureAwait(false);
            copiedFiles.Add(await BuildManifestFileRecordAsync(summaryPath, "analysis-manifest.txt", cancellationToken).ConfigureAwait(false));

            var manifestPath = Path.Combine(bundleDirectory, "analysis-manifest.json");
            copiedFiles.Add(new CrashAnalysisCopiedFile
            {
                SourcePath = manifestPath,
                BundleRelativePath = "analysis-manifest.json",
                SizeBytes = 0,
                Sha256 = "<self-referential>"
            });

            manifest.CopiedFiles = copiedFiles;
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken).ConfigureAwait(false);

            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Created crash analysis bundle {bundleDirectory} with {copiedFiles.Count} file(s) for phase {phase}");
            return bundleDirectory;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash analysis bundle creation failed non-fatally during {phase}: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<string> EnumerateCrashCompanionFiles(Settings settings, IReadOnlyCollection<string> sourceCrashFiles)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var sourceCrashFile in sourceCrashFiles.OrderBy(path => path, StringComparer.Ordinal))
        {
            if (seen.Add(sourceCrashFile) && File.Exists(sourceCrashFile))
            {
                yield return sourceCrashFile;
            }
        }

        var roundStateFiles = Directory.Exists(settings.CrashArtifactDirectory)
            ? Directory.EnumerateFiles(settings.CrashArtifactDirectory, "*active-round*.json", SearchOption.TopDirectoryOnly)
            : Array.Empty<string>();

        foreach (var roundStateFile in roundStateFiles.OrderBy(path => path, StringComparer.Ordinal))
        {
            var fullPath = Path.GetFullPath(roundStateFile);
            if (seen.Add(fullPath))
            {
                yield return fullPath;
            }
        }
    }

    private static IEnumerable<string> EnumerateAppFiles()
    {
        var appBaseDirectory = GetAppBaseDirectory();
        if (!Directory.Exists(appBaseDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var requiredFile in new[]
        {
            Path.Combine(appBaseDirectory, "uplink.NET.Repro.dll"),
            Path.Combine(appBaseDirectory, "uplink.NET.dll"),
            Path.Combine(appBaseDirectory, "uplink.NET.Repro.pdb"),
            Path.Combine(appBaseDirectory, "uplink.NET.pdb"),
            Path.Combine(appBaseDirectory, "runtimes", "linux-x64", "native", "storj_uplink.so")
        })
        {
            if (File.Exists(requiredFile) && seen.Add(requiredFile))
            {
                yield return requiredFile;
            }
        }

        // Crash artifacts can contain sensitive data. Only copy narrowly targeted debugging inputs from the app root,
        // never broad directories like /tmp or environment dumps that may contain secrets.
        foreach (var pattern in new[] { "*.dll", "*.pdb", "*.json", "*.deps.json", "*.runtimeconfig.json" })
        {
            foreach (var filePath in Directory.EnumerateFiles(appBaseDirectory, pattern, SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.Ordinal))
            {
                if (seen.Add(filePath))
                {
                    yield return filePath;
                }
            }
        }

        var nativeDirectory = Path.Combine(appBaseDirectory, "runtimes", "linux-x64", "native");
        if (!Directory.Exists(nativeDirectory))
        {
            yield break;
        }

        foreach (var filePath in Directory.EnumerateFiles(nativeDirectory, "*.so", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.Ordinal))
        {
            if (seen.Add(filePath))
            {
                yield return filePath;
            }
        }
    }

    private static IEnumerable<string> EnumerateRuntimeFiles()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var dotnetHostPath = ResolveDotnetHostPath();
        if (!string.IsNullOrWhiteSpace(dotnetHostPath) && File.Exists(dotnetHostPath) && seen.Add(dotnetHostPath))
        {
            yield return dotnetHostPath;
        }

        var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        foreach (var filePath in EnumerateExistingFiles(new[]
        {
            Path.Combine(runtimeDirectory, "libhostpolicy.so"),
            Path.Combine(runtimeDirectory, "libcoreclr.so"),
            Path.Combine(runtimeDirectory, "libclrjit.so"),
            Path.Combine(runtimeDirectory, "System.Private.CoreLib.dll"),
            Path.Combine(runtimeDirectory, "System.Runtime.dll")
        }))
        {
            if (seen.Add(filePath))
            {
                yield return filePath;
            }
        }

        foreach (var loadedLibrary in EnumerateExistingFiles(new[]
        {
            TryFindLoadedLibraryPath("libhostfxr.so"),
            TryFindLoadedLibraryPath("libhostpolicy.so"),
            TryFindLoadedLibraryPath("libcoreclr.so"),
            TryFindLoadedLibraryPath("libclrjit.so")
        }))
        {
            if (seen.Add(loadedLibrary))
            {
                yield return loadedLibrary;
            }
        }

        var hostFxrDirectory = TryResolveHostFxrDirectory(dotnetHostPath);
        if (!string.IsNullOrWhiteSpace(hostFxrDirectory))
        {
            foreach (var filePath in EnumerateExistingFiles(new[]
            {
                Path.Combine(hostFxrDirectory, "libhostfxr.so")
            }))
            {
                if (seen.Add(filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateExistingFiles(IEnumerable<string?> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                yield return Path.GetFullPath(candidate);
            }
        }
    }

    private static string GetAppBaseDirectory()
    {
        return Path.GetFullPath(AppContext.BaseDirectory);
    }

    private static string? ResolveDotnetHostPath()
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

    private static string? TryResolveHostFxrDirectory(string? dotnetHostPath)
    {
        var loadedHostFxr = TryFindLoadedLibraryPath("libhostfxr.so");
        if (!string.IsNullOrWhiteSpace(loadedHostFxr))
        {
            return Path.GetDirectoryName(loadedHostFxr);
        }

        if (string.IsNullOrWhiteSpace(dotnetHostPath))
        {
            return null;
        }

        var dotnetRoot = Path.GetDirectoryName(dotnetHostPath);
        if (string.IsNullOrWhiteSpace(dotnetRoot))
        {
            return null;
        }

        var fxrRoot = Path.Combine(dotnetRoot, "host", "fxr");
        if (!Directory.Exists(fxrRoot))
        {
            return null;
        }

        return Directory.EnumerateDirectories(fxrRoot)
            .OrderByDescending(path => path, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static string? TryFindLoadedLibraryPath(string fileName)
    {
        try
        {
            const string procMaps = "/proc/self/maps";
            if (!File.Exists(procMaps))
            {
                return null;
            }

            foreach (var line in File.ReadLines(procMaps))
            {
                var candidatePath = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                if (string.IsNullOrWhiteSpace(candidatePath) || !candidatePath.EndsWith(fileName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static string BuildBundleRelativePath(string rootPath, string filePath, string topLevelFolderName)
    {
        if (IsSubPathOf(filePath, rootPath))
        {
            return Path.Combine(topLevelFolderName, Path.GetRelativePath(rootPath, filePath));
        }

        return Path.Combine(topLevelFolderName, Path.GetFileName(filePath));
    }

    private static string BuildRuntimeBundlePath(string filePath)
    {
        var dotnetHostPath = ResolveDotnetHostPath();
        var dotnetRoot = string.IsNullOrWhiteSpace(dotnetHostPath) ? null : Path.GetDirectoryName(dotnetHostPath);
        if (!string.IsNullOrWhiteSpace(dotnetRoot) && IsSubPathOf(filePath, dotnetRoot))
        {
            return Path.Combine("dotnet", Path.GetRelativePath(dotnetRoot, filePath));
        }

        return Path.Combine("dotnet", Path.GetFileName(filePath));
    }

    private static bool IsSubPathOf(string filePath, string rootPath)
    {
        var normalizedFilePath = Path.GetFullPath(filePath);
        var normalizedRootPath = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return normalizedFilePath.StartsWith(normalizedRootPath, StringComparison.Ordinal);
    }

    private static bool IsUploadedPath(Settings settings, string path)
    {
        var uploadedDirectory = Path.GetFullPath(Path.Combine(settings.CrashArtifactDirectory, "uploaded"))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return path.StartsWith(uploadedDirectory, StringComparison.Ordinal);
    }

    private static bool IsBundlePath(Settings settings, string path)
    {
        if (!IsSubPathOf(path, settings.CrashArtifactDirectory))
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(settings.CrashArtifactDirectory, path);
        var firstSegment = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return firstSegment.StartsWith("bundle-", StringComparison.Ordinal);
    }

    private static bool IsCrashTriggerFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.StartsWith("coredump.", StringComparison.Ordinal)
            || fileName.StartsWith("createdump.", StringComparison.Ordinal)
            || fileName.EndsWith(".crashreport.json", StringComparison.Ordinal)
            || fileName.StartsWith("core", StringComparison.Ordinal);
    }

    private static async Task WaitForStableFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths)
        {
            await WaitForStableFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WaitForStableFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        FileSnapshot? previousSnapshot = null;
        var stableChecks = 0;

        for (var attempt = 0; attempt < StabilityMaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentSnapshot = TryGetSnapshot(filePath);
            if (currentSnapshot == null)
            {
                await Task.Delay(StabilityPollDelayMs, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (previousSnapshot != null
                && previousSnapshot.Length == currentSnapshot.Length
                && previousSnapshot.LastWriteTimeUtc == currentSnapshot.LastWriteTimeUtc)
            {
                stableChecks++;
                if (stableChecks >= StabilityChecksRequired)
                {
                    return;
                }
            }
            else
            {
                stableChecks = 0;
            }

            previousSnapshot = currentSnapshot;
            await Task.Delay(StabilityPollDelayMs, cancellationToken).ConfigureAwait(false);
        }
    }

    private static FileSnapshot? TryGetSnapshot(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return null;
            }

            return new FileSnapshot(fileInfo.Length, fileInfo.LastWriteTimeUtc);
        }
        catch
        {
            return null;
        }
    }

    private static Task<string> ComputeFingerprintAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        foreach (var filePath in filePaths.OrderBy(path => path, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileInfo = new FileInfo(filePath);
            builder.Append(filePath)
                .Append('|')
                .Append(fileInfo.Exists ? fileInfo.Length : 0)
                .Append('|')
                .Append(fileInfo.Exists ? fileInfo.LastWriteTimeUtc.Ticks : 0)
                .AppendLine();
        }

        return Task.FromResult(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant());
    }

    private static string? FindExistingBundleDirectory(Settings settings, string fingerprint)
    {
        if (!Directory.Exists(settings.CrashArtifactDirectory))
        {
            return null;
        }

        foreach (var bundleDirectory in Directory.EnumerateDirectories(settings.CrashArtifactDirectory, "bundle-*", SearchOption.TopDirectoryOnly))
        {
            var manifestPath = Path.Combine(bundleDirectory, "analysis-manifest.json");
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                var manifest = JsonSerializer.Deserialize<CrashAnalysisManifest>(File.ReadAllText(manifestPath));
                if (string.Equals(manifest?.CrashSourceFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    return bundleDirectory;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static int? TryExtractProcessId(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var parts = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (int.TryParse(part, out var pid) && pid > 0)
            {
                return pid;
            }
        }

        return null;
    }

    private static async Task<CrashAnalysisCopiedFile?> TryCopyFileIntoBundleAsync(
        string bundleDirectory,
        string sourcePath,
        string bundleRelativePath,
        IReadOnlyCollection<CrashAnalysisCopiedFile> existingFiles,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return null;
            }

            var normalizedRelativePath = bundleRelativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (existingFiles.Any(file => string.Equals(file.BundleRelativePath, normalizedRelativePath.Replace(Path.DirectorySeparatorChar, '/'), StringComparison.Ordinal)))
            {
                return null;
            }

            var destinationPath = Path.Combine(bundleDirectory, normalizedRelativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            await using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1024 * 1024, useAsync: true))
            await using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
            }

            return await BuildCopiedFileRecordAsync(sourcePath, destinationPath, normalizedRelativePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTimeOffset.UtcNow:O}] Crash bundle copy skipped non-fatally for {sourcePath}: {ex.Message}");
            return null;
        }
    }

    private static async Task<CrashAnalysisCopiedFile> BuildCopiedFileRecordAsync(string sourcePath, string destinationPath, string bundleRelativePath, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(destinationPath);
        return new CrashAnalysisCopiedFile
        {
            SourcePath = sourcePath,
            BundleRelativePath = bundleRelativePath.Replace(Path.DirectorySeparatorChar, '/'),
            SizeBytes = fileInfo.Length,
            Sha256 = await ComputeSha256Async(destinationPath, cancellationToken).ConfigureAwait(false),
            LinkTarget = TryGetLinkTarget(sourcePath)
        };
    }

    private static async Task<CrashAnalysisCopiedFile> BuildManifestFileRecordAsync(string manifestPath, string bundleRelativePath, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(manifestPath);
        return new CrashAnalysisCopiedFile
        {
            SourcePath = manifestPath,
            BundleRelativePath = bundleRelativePath,
            SizeBytes = fileInfo.Length,
            Sha256 = await ComputeSha256Async(manifestPath, cancellationToken).ConfigureAwait(false)
        };
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string? TryGetLinkTarget(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            return fileInfo.LinkTarget;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string> TryGetDotnetInfoAsync(CancellationToken cancellationToken)
    {
        var dotnetHostPath = ResolveDotnetHostPath() ?? "dotnet";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = dotnetHostPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            startInfo.ArgumentList.Add("--info");

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return "dotnet --info could not be started.";
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var output = await outputTask.ConfigureAwait(false);
            var error = await errorTask.ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(error) ? output : $"{output}{Environment.NewLine}{error}";
        }
        catch (Exception ex)
        {
            return $"dotnet --info failed non-fatally: {ex.Message}";
        }
    }

    private static string BuildManifestSummary(CrashAnalysisManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"BundleCreatedAtUtc: {manifest.BundleCreatedAtUtc:O}");
        builder.AppendLine($"Phase: {manifest.Phase}");
        builder.AppendLine($"Hostname: {manifest.Hostname}");
        builder.AppendLine($"CrashPid: {manifest.ProcessId}");
        builder.AppendLine($"CurrentProcessId: {manifest.CurrentProcessId}");
        builder.AppendLine($"CommandLine: {manifest.CommandLine}");
        builder.AppendLine($"Environment.Version: {manifest.EnvironmentVersion}");
        builder.AppendLine($"FrameworkDescription: {manifest.FrameworkDescription}");
        builder.AppendLine($"OSDescription: {manifest.OsDescription}");
        builder.AppendLine($"ProcessArchitecture: {manifest.ProcessArchitecture}");
        builder.AppendLine($"CrashSourceFingerprint: {manifest.CrashSourceFingerprint}");
        builder.AppendLine();
        builder.AppendLine("Copied files:");

        foreach (var copiedFile in manifest.CopiedFiles.OrderBy(file => file.BundleRelativePath, StringComparer.Ordinal))
        {
            builder.AppendLine($"- {copiedFile.BundleRelativePath}");
            builder.AppendLine($"  source: {copiedFile.SourcePath}");
            builder.AppendLine($"  size: {copiedFile.SizeBytes}");
            builder.AppendLine($"  sha256: {copiedFile.Sha256}");
            if (!string.IsNullOrWhiteSpace(copiedFile.LinkTarget))
            {
                builder.AppendLine($"  linkTarget: {copiedFile.LinkTarget}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("dotnet --info:");
        builder.AppendLine(manifest.DotnetInfo);
        return builder.ToString();
    }

    private sealed record FileSnapshot(long Length, DateTime LastWriteTimeUtc);
}

internal sealed class CrashAnalysisManifest
{
    public DateTimeOffset BundleCreatedAtUtc { get; init; }
    public string Phase { get; init; } = string.Empty;
    public string Hostname { get; init; } = string.Empty;
    public int ProcessId { get; init; }
    public int CurrentProcessId { get; init; }
    public string CommandLine { get; init; } = string.Empty;
    public string EnvironmentVersion { get; init; } = string.Empty;
    public string FrameworkDescription { get; init; } = string.Empty;
    public string OsDescription { get; init; } = string.Empty;
    public string ProcessArchitecture { get; init; } = string.Empty;
    public string DotnetInfo { get; init; } = string.Empty;
    public string CrashSourceFingerprint { get; init; } = string.Empty;
    public List<string> SourceCrashFiles { get; init; } = new();
    public List<CrashAnalysisCopiedFile> CopiedFiles { get; set; } = new();
}

internal sealed class CrashAnalysisCopiedFile
{
    public string SourcePath { get; init; } = string.Empty;
    public string BundleRelativePath { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string? LinkTarget { get; init; }
}
