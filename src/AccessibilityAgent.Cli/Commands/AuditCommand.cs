using Spectre.Console;
using AccessibilityAgent.Core.Engine;
using AccessibilityAgent.Core.Models;
using AccessibilityAgent.Core.Reporters;

namespace AccessibilityAgent.Cli.Commands;

public static class AuditCommand
{
    public static async Task<int> ExecuteAsync(
        string target,
        string format = "cli",
        string? output = null,
        string level = "AA",
        string failOn = "critical")
    {
        var engine = new AccessibilityEngine();
        var wcagLevel = level.ToUpperInvariant() switch
        {
            "A" => WcagLevel.A,
            "AAA" => WcagLevel.AAA,
            _ => WcagLevel.AA
        };

        var options = new AccessibilityEngineOptions { Level = wcagLevel };
        AuditResult result;

        try
        {
            if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                await AnsiConsole.Status().StartAsync($"Auditing live URL {target}...", async ctx =>
                {
                    result = await engine.AuditUrlAsync(target, options);
                });
                result = await engine.AuditUrlAsync(target, options);
            }
            else if (Directory.Exists(target))
            {
                var files = Directory.GetFiles(target, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (files.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]No HTML or Razor files found in {target}[/]");
                    return 0;
                }

                int totalExit = 0;
                foreach (var file in files)
                {
                    var fileResult = await engine.AuditFileAsync(file, options);
                    SpectreReporter.RenderAuditResult(fileResult);
                    if (ShouldFail(fileResult, failOn)) totalExit = 1;
                }
                return totalExit;
            }
            else if (File.Exists(target))
            {
                result = await engine.AuditFileAsync(target, options);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Target not found: {target}[/]");
                return 1;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Audit failed: {Markup.Escape(ex.Message)}[/]");
            return 1;
        }

        // Output formatting
        if (format.Equals("html", StringComparison.OrdinalIgnoreCase) || (output?.EndsWith(".html") ?? false))
        {
            var htmlReporter = new HtmlReporter();
            var html = await htmlReporter.GenerateReportAsync(result);
            var outFile = output ?? "a11y-report.html";
            await File.WriteAllTextAsync(outFile, html);
            AnsiConsole.MarkupLine($"[green]HTML report saved to {outFile}[/]");
        }
        else if (format.Equals("markdown", StringComparison.OrdinalIgnoreCase) || (output?.EndsWith(".md") ?? false))
        {
            var mdReporter = new MarkdownReporter();
            var md = await mdReporter.GenerateReportAsync(result);
            var outFile = output ?? "a11y-report.md";
            await File.WriteAllTextAsync(outFile, md);
            AnsiConsole.MarkupLine($"[green]Markdown report saved to {outFile}[/]");
        }
        else if (format.Equals("sarif", StringComparison.OrdinalIgnoreCase) || (output?.EndsWith(".sarif") ?? false))
        {
            var sarifReporter = new SarifReporter();
            var sarif = await sarifReporter.GenerateReportAsync(result);
            var outFile = output ?? "a11y-report.sarif";
            await File.WriteAllTextAsync(outFile, sarif);
            AnsiConsole.MarkupLine($"[green]SARIF report saved to {outFile}[/]");
        }
        else
        {
            SpectreReporter.RenderAuditResult(result);
        }

        return ShouldFail(result, failOn) ? 1 : 0;
    }

    private static bool ShouldFail(AuditResult result, string failOn) => failOn.ToLowerInvariant() switch
    {
        "critical" => result.CriticalCount > 0,
        "serious" => result.CriticalCount > 0 || result.SeriousCount > 0,
        "moderate" => result.CriticalCount > 0 || result.SeriousCount > 0 || result.ModerateCount > 0,
        "minor" => result.TotalIssues > 0,
        _ => result.CriticalCount > 0
    };
}
