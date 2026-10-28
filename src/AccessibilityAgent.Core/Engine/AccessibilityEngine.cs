using System.Diagnostics;
using AngleSharp;
using AngleSharp.Html.Parser;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Models;
using AccessibilityAgent.Core.Rules;

namespace AccessibilityAgent.Core.Engine;

public class AccessibilityEngineOptions
{
    public WcagLevel Level { get; set; } = WcagLevel.AA;
    public IssueSeverity MinSeverity { get; set; } = IssueSeverity.Info;
    public RuleCategory? Category { get; set; }
    public bool IncludeTree { get; set; } = true;
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(15);
}

public class AccessibilityEngine
{
    private readonly RuleRegistry _registry;
    private readonly HttpClient _httpClient;

    public AccessibilityEngine(RuleRegistry? registry = null, HttpClient? httpClient = null)
    {
        _registry = registry ?? new RuleRegistry();
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<AuditResult> AuditHtmlAsync(
        string htmlContent,
        string targetName = "InlineHTML",
        AccessibilityEngineOptions? options = null)
    {
        options ??= new AccessibilityEngineOptions();
        var sw = Stopwatch.StartNew();

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var parser = context.GetService<IHtmlParser>() ?? new HtmlParser();
        var document = await parser.ParseDocumentAsync(htmlContent);

        var rules = _registry.GetRules(options.Level, options.MinSeverity, options.Category).ToList();
        var issues = new List<AccessibilityIssue>();

        foreach (var rule in rules)
        {
            try
            {
                var ruleIssues = rule.Evaluate(document, htmlContent);
                foreach (var issue in ruleIssues)
                {
                    issues.Add(issue with { TargetFile = targetName });
                }
            }
            catch (Exception ex)
            {
                // Resilient rule execution
                Debug.WriteLine($"Error evaluating rule {rule.Id}: {ex.Message}");
            }
        }

        sw.Stop();

        var result = new AuditResult
        {
            Target = targetName,
            Timestamp = DateTimeOffset.UtcNow,
            Duration = sw.Elapsed,
            ElementsEvaluated = document.All.Length,
            RulesEvaluated = rules.Count,
            Issues = issues.OrderByDescending(i => i.Severity).ToList(),
            AccessibilityTree = options.IncludeTree ? AccessibilityTreeBuilder.BuildTree(document) : null
        };

        return result;
    }

    public async Task<AuditResult> AuditFileAsync(string filePath, AccessibilityEngineOptions? options = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Target file not found: {filePath}", filePath);

        var content = await File.ReadAllTextAsync(filePath);
        return await AuditHtmlAsync(content, filePath, options);
    }

    public async Task<AuditResult> AuditUrlAsync(string url, AccessibilityEngineOptions? options = null)
    {
        options ??= new AccessibilityEngineOptions();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));

        using var cts = new CancellationTokenSource(options.HttpTimeout);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("User-Agent", "AccessibilityAgent/1.0 (WCAG-Audit-Bot)");

        var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cts.Token);
        return await AuditHtmlAsync(content, url, options);
    }
}
