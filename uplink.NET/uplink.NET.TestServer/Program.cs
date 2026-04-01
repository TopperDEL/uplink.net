using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TestRunService>();

var app = builder.Build();

app.MapGet("/", (TestRunService service) => Results.Content(HtmlPage.Render(service.HasDefaultAccessGrant), "text/html; charset=utf-8"));

app.MapPost("/api/test-runs", (StartTestRunRequest request, TestRunService service) =>
{
    var result = service.Start(request.AccessGrant);
    return result.Started
        ? Results.Accepted($"/api/test-runs/{result.Run.Id}", result.Run)
        : Results.Conflict(result.Run);
});

app.MapGet("/api/test-runs/{id:guid}", (Guid id, TestRunService service) =>
{
    var run = service.Get(id);
    return run is null ? Results.NotFound() : Results.Ok(run);
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();

internal sealed class TestRunService
{
    private readonly object syncLock = new();
    private readonly ILogger<TestRunService> logger;
    private readonly Dictionary<Guid, TestRunState> runs = new();
    private Guid? activeRunId;

    public TestRunService(ILogger<TestRunService> logger)
    {
        this.logger = logger;
    }

    public bool HasDefaultAccessGrant => !string.IsNullOrWhiteSpace(GetDefaultAccessGrant());

    public StartTestRunResult Start(string? accessGrant)
    {
        accessGrant = string.IsNullOrWhiteSpace(accessGrant) ? GetDefaultAccessGrant() : accessGrant.Trim();
        if (string.IsNullOrWhiteSpace(accessGrant))
        {
            throw new BadHttpRequestException("Provide an access grant in the request body, or configure the server-side UPLINK_TEST_ACCESS_GRANT environment variable.", StatusCodes.Status400BadRequest);
        }

        lock (syncLock)
        {
            if (activeRunId is Guid runningId && runs.TryGetValue(runningId, out var existing) && existing.Status == "running")
            {
                return new StartTestRunResult(false, existing.ToSnapshot());
            }

            var run = new TestRunState
            {
                Id = Guid.NewGuid(),
                Status = "running",
                Summary = "Running tests...",
                StartedAtUtc = DateTimeOffset.UtcNow,
                AccessGrantConfigured = true,
            };

            runs[run.Id] = run;
            activeRunId = run.Id;
            _ = ExecuteAsync(run, accessGrant);
            return new StartTestRunResult(true, run.ToSnapshot());
        }
    }

    public TestRunSnapshot? Get(Guid id)
    {
        lock (syncLock)
        {
            return runs.TryGetValue(id, out var run) ? run.ToSnapshot() : null;
        }
    }

    private async Task ExecuteAsync(TestRunState run, string accessGrant)
    {
        var resultsRoot = Path.Combine(Path.GetTempPath(), "uplink-net-test-runs", run.Id.ToString("N"));
        Directory.CreateDirectory(resultsRoot);

        var trxFileName = $"{run.Id:N}.trx";
        var testAssemblyPath = Environment.GetEnvironmentVariable("UPLINK_TEST_ASSEMBLY_PATH") ?? "/app/tests/uplink.NET.Test.dll";

        using var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.ArgumentList.Add("test");
        process.StartInfo.ArgumentList.Add(testAssemblyPath);
        process.StartInfo.ArgumentList.Add("--no-build");
        process.StartInfo.ArgumentList.Add("--logger");
        process.StartInfo.ArgumentList.Add($"trx;LogFileName={trxFileName}");
        process.StartInfo.ArgumentList.Add("--results-directory");
        process.StartInfo.ArgumentList.Add(resultsRoot);
        process.StartInfo.ArgumentList.Add("--verbosity");
        process.StartInfo.ArgumentList.Add("minimal");
        process.StartInfo.Environment["UPLINK_TEST_ACCESS_GRANT"] = accessGrant;
        CopyEnvironmentVariable(process.StartInfo.Environment, "UPLINK_TEST_API_KEY");
        CopyEnvironmentVariable(process.StartInfo.Environment, "UPLINK_TEST_ENCRYPTION_SECRET");
        CopyEnvironmentVariable(process.StartInfo.Environment, "UPLINK_TEST_INVALID_API_KEY");
        CopyEnvironmentVariable(process.StartInfo.Environment, "UPLINK_TEST_SATELLITE_URL");

        var output = new StringBuilder();
        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (output)
                {
                    output.AppendLine(args.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (output)
                {
                    output.AppendLine(args.Data);
                }
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            var trxPath = Path.Combine(resultsRoot, trxFileName);
            var report = File.Exists(trxPath)
                ? TrxParser.Parse(trxPath)
                : TrxReport.Empty(process.ExitCode == 0 ? "Tests finished without a TRX file." : $"dotnet test failed with exit code {process.ExitCode} before producing a TRX file.");

            lock (syncLock)
            {
                run.CompletedAtUtc = DateTimeOffset.UtcNow;
                run.ExitCode = process.ExitCode;
                run.Status = process.ExitCode == 0 && report.Failed == 0 ? "passed" : "failed";
                run.Summary = $"{report.Passed} passed, {report.Failed} failed, {report.Skipped} skipped";
                run.Total = report.Total;
                run.Passed = report.Passed;
                run.Failed = report.Failed;
                run.Skipped = report.Skipped;
                run.Output = output.ToString().Trim();
                run.Results = report.Results;
                activeRunId = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test run {RunId} failed.", run.Id);
            lock (syncLock)
            {
                run.CompletedAtUtc = DateTimeOffset.UtcNow;
                run.Status = "failed";
                run.Summary = ex.Message;
                run.Output = ex.ToString();
                run.Results = Array.Empty<TestCaseSnapshot>();
                activeRunId = null;
            }
        }
    }

    private static string? GetDefaultAccessGrant()
    {
        return Environment.GetEnvironmentVariable("UPLINK_TEST_ACCESS_GRANT")?.Trim();
    }

    private static void CopyEnvironmentVariable(IDictionary<string, string?> destination, string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            destination[name] = value;
        }
    }
}

internal static class TrxParser
{
    public static TrxReport Parse(string trxPath)
    {
        var document = XDocument.Load(trxPath);
        var ns = document.Root?.Name.Namespace ?? XNamespace.None;
        var counters = document.Descendants(ns + "Counters").FirstOrDefault();
        var results = document.Descendants(ns + "UnitTestResult")
            .Select(result =>
            {
                var output = result.Element(ns + "Output");
                var errorInfo = output?.Element(ns + "ErrorInfo");
                var message = errorInfo?.Element(ns + "Message")?.Value ?? output?.Element(ns + "StdOut")?.Value ?? string.Empty;
                return new TestCaseSnapshot(
                    result.Attribute("testName")?.Value ?? "Unknown test",
                    (result.Attribute("outcome")?.Value ?? "Unknown").ToLowerInvariant(),
                    result.Attribute("duration")?.Value,
                    string.IsNullOrWhiteSpace(message) ? null : message.Trim());
            })
            .OrderBy(result => result.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new TrxReport(
            ParseCounter(counters, "total"),
            ParseCounter(counters, "passed"),
            ParseCounter(counters, "failed"),
            ParseSkippedCount(counters),
            results,
            null);
    }

    private static int ParseCounter(XElement? counters, string name)
    {
        return int.TryParse(counters?.Attribute(name)?.Value, out var value) ? value : 0;
    }

    private static int ParseSkippedCount(XElement? counters)
    {
        return ParseCounter(counters, "notExecuted") + ParseCounter(counters, "warning") + ParseCounter(counters, "inconclusive");
    }
}

internal static class HtmlPage
{
    public static string Render(bool hasDefaultAccessGrant)
    {
        var autoRun = hasDefaultAccessGrant ? "true" : "false";
        var defaultHint = hasDefaultAccessGrant
            ? "A default access grant is configured, so the tests will start automatically."
            : "Paste an access grant to start a run.";

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>uplink.NET test runner</title>
  <style>
    :root { color-scheme: dark; }
    body { font-family: Arial, sans-serif; margin: 0; background: #111827; color: #f9fafb; }
    main { max-width: 900px; margin: 0 auto; padding: 2rem 1rem 4rem; }
    .card { background: #1f2937; border-radius: 12px; padding: 1rem 1.25rem; margin-bottom: 1rem; box-shadow: 0 10px 30px rgba(0,0,0,.2); }
    textarea { width: 100%; min-height: 8rem; resize: vertical; border-radius: 8px; border: 1px solid #374151; background: #0f172a; color: #f9fafb; padding: .75rem; box-sizing: border-box; }
    button { background: #2563eb; border: 0; color: white; border-radius: 8px; padding: .75rem 1rem; font-weight: 600; cursor: pointer; }
    button:disabled { opacity: .65; cursor: wait; }
    .status { display: inline-block; border-radius: 999px; padding: .35rem .7rem; font-weight: 700; text-transform: uppercase; letter-spacing: .04em; font-size: .75rem; }
    .status.running { background: #1d4ed8; }
    .status.passed { background: #15803d; }
    .status.failed { background: #b91c1c; }
    .muted { color: #cbd5e1; }
    .summary { font-size: 1.1rem; margin: .75rem 0; }
    .test { border-top: 1px solid #374151; padding: .75rem 0; }
    .test:first-child { border-top: 0; }
    pre { white-space: pre-wrap; overflow-wrap: anywhere; background: #020617; padding: .75rem; border-radius: 8px; }
  </style>
</head>
<body>
  <main>
    <div class="card">
      <h1>uplink.NET Fly.io test runner</h1>
      <p class="muted">{{WebUtility.HtmlEncode(defaultHint)}}</p>
      <textarea id="accessGrant" placeholder="Access grant (optional when UPLINK_TEST_ACCESS_GRANT is configured)"></textarea>
      <div style="margin-top: 1rem; display: flex; gap: .75rem; align-items: center; flex-wrap: wrap;">
        <button id="runButton">Run tests</button>
        <span id="message" class="muted"></span>
      </div>
    </div>
    <div id="result" class="card" hidden>
      <div id="badge" class="status running">running</div>
      <div id="summary" class="summary">Waiting for a test run.</div>
      <div id="counts" class="muted"></div>
      <div id="tests"></div>
      <div id="logContainer" hidden>
        <h2>dotnet test output</h2>
        <pre id="log"></pre>
      </div>
    </div>
  </main>
  <script>
    const runButton = document.getElementById('runButton');
    const accessGrant = document.getElementById('accessGrant');
    const message = document.getElementById('message');
    const result = document.getElementById('result');
    const badge = document.getElementById('badge');
    const summary = document.getElementById('summary');
    const counts = document.getElementById('counts');
    const tests = document.getElementById('tests');
    const logContainer = document.getElementById('logContainer');
    const log = document.getElementById('log');
    let pollHandle = null;

    function escapeHtml(value) {
      return value
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
    }

    function render(run) {
      result.hidden = false;
      badge.textContent = run.status;
      badge.className = `status ${run.status}`;
      summary.textContent = run.summary;
      counts.textContent = `Total: ${run.total} · Passed: ${run.passed} · Failed: ${run.failed} · Skipped: ${run.skipped}`;
      tests.innerHTML = run.results.map(test => `
        <div class="test">
          <strong>${escapeHtml(test.name)}</strong>
          <div class="muted">${escapeHtml(test.outcome)}${test.duration ? ` · ${escapeHtml(test.duration)}` : ''}</div>
          ${test.message ? `<pre>${escapeHtml(test.message)}</pre>` : ''}
        </div>`).join('');
      if (run.output) {
        logContainer.hidden = false;
        log.textContent = run.output;
      } else {
        logContainer.hidden = true;
        log.textContent = '';
      }
      runButton.disabled = run.status === 'running';
      message.textContent = run.status === 'running' ? 'Test run in progress...' : '';
    }

    async function poll(id) {
      if (pollHandle) {
        clearTimeout(pollHandle);
      }

      const response = await fetch(`/api/test-runs/${id}`);
      if (!response.ok) {
        message.textContent = `Could not refresh the test run status (HTTP ${response.status}).`;
        runButton.disabled = false;
        return;
      }

      const run = await response.json();
      render(run);
      if (run.status === 'running') {
        pollHandle = setTimeout(() => poll(id), 1500);
      }
    }

    async function startRun() {
      runButton.disabled = true;
      message.textContent = 'Starting test run...';

      const response = await fetch('/api/test-runs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accessGrant: accessGrant.value })
      });

      if (response.status === 400) {
        const error = await response.text();
        message.textContent = error;
        runButton.disabled = false;
        return;
      }

      const run = await response.json();
      render(run);
      await poll(run.id);
    }

    runButton.addEventListener('click', () => {
      startRun().catch(error => {
        console.error(error);
        message.textContent = 'Failed to start the test run.';
        runButton.disabled = false;
      });
    });

    if ({{autoRun}}) {
      startRun().catch(error => {
        console.error(error);
        message.textContent = 'Failed to start the automatic test run.';
        runButton.disabled = false;
      });
    }
  </script>
</body>
</html>
""";
    }
}

internal sealed class TestRunState
{
    public Guid Id { get; init; }
    public string Status { get; set; } = "queued";
    public string Summary { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public int Total { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public bool AccessGrantConfigured { get; set; }
    public DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? Output { get; set; }
    public IReadOnlyList<TestCaseSnapshot> Results { get; set; } = Array.Empty<TestCaseSnapshot>();

    public TestRunSnapshot ToSnapshot()
        => new(Id, Status, Summary, ExitCode, Total, Passed, Failed, Skipped, AccessGrantConfigured, StartedAtUtc, CompletedAtUtc, Results, Output);
}

internal sealed record StartTestRunRequest(string? AccessGrant);
internal sealed record StartTestRunResult(bool Started, TestRunSnapshot Run);
internal sealed record TestRunSnapshot(Guid Id, string Status, string Summary, int ExitCode, int Total, int Passed, int Failed, int Skipped, bool AccessGrantConfigured, DateTimeOffset StartedAtUtc, DateTimeOffset? CompletedAtUtc, IReadOnlyList<TestCaseSnapshot> Results, string? Output);
internal sealed record TestCaseSnapshot(string Name, string Outcome, string? Duration, string? Message);
internal sealed record TrxReport(int Total, int Passed, int Failed, int Skipped, IReadOnlyList<TestCaseSnapshot> Results, string? Message)
{
    public static TrxReport Empty(string message) => new(0, 0, 0, 0, Array.Empty<TestCaseSnapshot>(), message);
}
