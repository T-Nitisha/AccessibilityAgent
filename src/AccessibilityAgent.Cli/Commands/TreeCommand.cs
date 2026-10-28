using Spectre.Console;
using AccessibilityAgent.Core.Engine;

namespace AccessibilityAgent.Cli.Commands;

public static class TreeCommand
{
    public static async Task<int> ExecuteAsync(string target)
    {
        var engine = new AccessibilityEngine();
        try
        {
            var result = target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         target.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? await engine.AuditUrlAsync(target)
                : await engine.AuditFileAsync(target);

            if (result.AccessibilityTree == null)
            {
                AnsiConsole.MarkupLine("[yellow]No accessibility tree generated.[/]");
                return 0;
            }

            SpectreReporter.RenderAccessibilityTree(result.AccessibilityTree);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating accessibility tree: {Markup.Escape(ex.Message)}[/]");
            return 1;
        }
    }
}
