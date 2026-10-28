using Spectre.Console;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Cli;

public static class SpectreReporter
{
    public static void RenderAuditResult(AuditResult result)
    {
        AnsiConsole.WriteLine();
        var rule = new Rule($"[bold yellow]♿ Accessibility Audit: {Markup.Escape(result.Target)}[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Top summary grid
        var scoreColor = result.AccessibilityScore switch
        {
            >= 90 => "green",
            >= 75 => "blue",
            >= 60 => "yellow",
            _ => "red"
        };

        var summaryTable = new Table().Border(TableBorder.Rounded);
        summaryTable.AddColumn(new TableColumn("[bold]Metric[/]").Centered());
        summaryTable.AddColumn(new TableColumn("[bold]Result[/]").Centered());
        summaryTable.AddColumn(new TableColumn("[bold]WCAG Standard[/]").Centered());
        summaryTable.AddColumn(new TableColumn("[bold]Status[/]").Centered());

        var aaStatus = result.PassesWcagAa ? "[bold green]PASS[/]" : "[bold red]FAIL[/]";
        var aaaStatus = result.PassesWcagAaa ? "[bold green]PASS[/]" : "[bold yellow]NEEDS WORK[/]";

        summaryTable.AddRow("Accessibility Score", $"[{scoreColor} bold]{result.AccessibilityScore}/100 (Grade: {result.Grade})[/]", "WCAG 2.1 / 2.2 AA", aaStatus);
        summaryTable.AddRow("Total Violations", $"[bold]{result.TotalIssues}[/]", "WCAG 2.1 / 2.2 AAA", aaaStatus);
        summaryTable.AddRow("Elements Evaluated", $"{result.ElementsEvaluated}", "Duration", $"{result.Duration.TotalMilliseconds:F0} ms");

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        // Severity breakdown bar
        var sevTable = new Table().Border(TableBorder.Simple);
        sevTable.AddColumn(new TableColumn("[bold red]🔴 Critical[/]").Centered());
        sevTable.AddColumn(new TableColumn("[bold orange3]🟠 Serious[/]").Centered());
        sevTable.AddColumn(new TableColumn("[bold yellow]🟡 Moderate[/]").Centered());
        sevTable.AddColumn(new TableColumn("[bold blue]🔵 Minor[/]").Centered());
        sevTable.AddRow(
            $"[red bold]{result.CriticalCount}[/]",
            $"[orange3 bold]{result.SeriousCount}[/]",
            $"[yellow bold]{result.ModerateCount}[/]",
            $"[blue bold]{result.MinorCount}[/]"
        );
        AnsiConsole.Write(sevTable);
        AnsiConsole.WriteLine();

        if (result.Issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[bold green]✔ Congratulations! No accessibility violations found.[/]");
            return;
        }

        // List each issue
        int index = 1;
        foreach (var issue in result.Issues)
        {
            var (sevColor, sevLabel) = issue.Severity switch
            {
                IssueSeverity.Critical => ("red", "CRITICAL"),
                IssueSeverity.Serious => ("orange3", "SERIOUS"),
                IssueSeverity.Moderate => ("yellow", "MODERATE"),
                _ => ("blue", "MINOR")
            };

            var panel = new Panel(new Markup(
                $"[{sevColor} bold]{sevLabel}[/] &bull; [bold white]{Markup.Escape(issue.RuleName)}[/] [dim]({issue.RuleId})[/]\n" +
                $"[cyan]Location:[/] {Markup.Escape(issue.FormattedLocation)}  [cyan]Selector:[/] [dim]{Markup.Escape(issue.Selector)}[/]\n" +
                $"[cyan]WCAG:[/] [underline blue]{issue.Criterion.Number} {issue.Criterion.Name} (Level {issue.Criterion.Level})[/]\n\n" +
                $"[bold]Issue:[/] {Markup.Escape(issue.Message)}\n\n" +
                $"[green bold]Remediation:[/] {Markup.Escape(issue.RemediationSummary)}" +
                (string.IsNullOrEmpty(issue.HtmlSnippet) ? "" : $"\n\n[dim]Snippet:[/] [grey78]{Markup.Escape(issue.HtmlSnippet)}[/]") +
                (string.IsNullOrEmpty(issue.SuggestedFixCode) ? "" : $"\n[bold green]Suggested Code Fix:[/] [green]{Markup.Escape(issue.SuggestedFixCode)}[/]")
            ))
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader($"Issue #{index++}")
            };

            AnsiConsole.Write(panel);
        }
    }

    public static void RenderAccessibilityTree(AccessibilityNode root)
    {
        var tree = new Tree($"[bold cyan]🌳 Accessibility Object Model (AOM)[/]");
        AddNodeToTree(tree, root);
        AnsiConsole.Write(tree);
    }

    private static void AddNodeToTree(IHasTreeNodes parent, AccessibilityNode node)
    {
        var role = $"[bold magenta]{node.Role}[/]";
        var name = string.IsNullOrWhiteSpace(node.Name) ? "" : $" [green]\"{Markup.Escape(node.Name)}\"[/]";
        var stateStr = node.States.Any() ? $" [yellow][[{string.Join(", ", node.States.Select(s => $"{s.Key}={s.Value}"))}]][/]" : "";
        var focusable = node.Focusable ? " [blue][[focusable]][/]" : "";

        var treeNode = parent.AddNode($"{role}{name}{stateStr}{focusable}");

        foreach (var child in node.Children.Take(30))
        {
            AddNodeToTree(treeNode, child);
        }
    }
}
