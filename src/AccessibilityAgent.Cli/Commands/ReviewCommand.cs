using Spectre.Console;
using AccessibilityAgent.Core.Engine;
using AccessibilityAgent.Core.Models;
using AccessibilityAgent.Core.Reporters;

namespace AccessibilityAgent.Cli.Commands;

public static class ReviewCommand
{
    public static async Task<int> ExecuteAsync(string path, string? commentOutput = null, string? patchOutput = null)
    {
        AnsiConsole.MarkupLine("[bold cyan]🔍 Accessibility Agent: PR & Code Review Mode[/]");
        AnsiConsole.MarkupLine($"[dim]Scanning files in: {path}[/]\n");

        var engine = new AccessibilityEngine();
        var filesToReview = new List<string>();

        if (File.Exists(path))
        {
            filesToReview.Add(path);
        }
        else if (Directory.Exists(path))
        {
            filesToReview.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Path not found: {path}[/]");
            return 1;
        }

        var aggregatedIssues = new List<AccessibilityIssue>();
        var aggregatedResults = new List<AuditResult>();

        foreach (var file in filesToReview)
        {
            var result = await engine.AuditFileAsync(file);
            if (result.Issues.Count > 0)
            {
                aggregatedResults.Add(result);
                aggregatedIssues.AddRange(result.Issues);
            }
        }

        if (aggregatedIssues.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold green]✔ Code Review Passed! No accessibility violations found in reviewed files.[/]");
            return 0;
        }

        // Display Reviewer Summary
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold]File[/]");
        table.AddColumn("[bold]Line[/]");
        table.AddColumn("[bold]Severity[/]");
        table.AddColumn("[bold]Violation[/]");
        table.AddColumn("[bold]Suggested Remediation[/]");

        foreach (var issue in aggregatedIssues)
        {
            var sevColor = issue.Severity switch
            {
                IssueSeverity.Critical => "red bold",
                IssueSeverity.Serious => "orange3 bold",
                IssueSeverity.Moderate => "yellow",
                _ => "blue"
            };

            table.AddRow(
                Path.GetFileName(issue.TargetFile ?? "Unknown"),
                $"Line {issue.LineNumber}",
                $"[{sevColor}]{issue.Severity}[/]",
                Markup.Escape(issue.RuleName),
                Markup.Escape(issue.RemediationSummary)
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // If markdown comment output requested (for GitHub PR action)
        if (!string.IsNullOrEmpty(commentOutput))
        {
            var mdReporter = new MarkdownReporter();
            var dummyAggregatedResult = new AuditResult
            {
                Target = path,
                ElementsEvaluated = aggregatedResults.Sum(r => r.ElementsEvaluated),
                Duration = TimeSpan.FromMilliseconds(aggregatedResults.Sum(r => r.Duration.TotalMilliseconds)),
                Issues = aggregatedIssues
            };

            var md = await mdReporter.GenerateReportAsync(dummyAggregatedResult);
            await File.WriteAllTextAsync(commentOutput, md);
            AnsiConsole.MarkupLine($"[green]✔ Review comments markdown written to {commentOutput} (ready for GitHub PR comment!)[/]");
        }

        // If patch output requested
        if (!string.IsNullOrEmpty(patchOutput))
        {
            var patch = string.Join("\n\n", aggregatedResults.Select(RemediationEngine.GenerateReviewPatch));
            await File.WriteAllTextAsync(patchOutput, patch);
            AnsiConsole.MarkupLine($"[green]✔ Unified review patch written to {patchOutput}[/]");
        }

        return aggregatedIssues.Any(i => i.Severity == IssueSeverity.Critical || i.Severity == IssueSeverity.Serious) ? 1 : 0;
    }
}
