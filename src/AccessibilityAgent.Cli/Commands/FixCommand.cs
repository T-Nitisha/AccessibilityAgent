using Spectre.Console;
using AccessibilityAgent.Core.Engine;

namespace AccessibilityAgent.Cli.Commands;

public static class FixCommand
{
    public static async Task<int> ExecuteAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Attempting automatic accessibility remediation for: {filePath}[/]");
        var engine = new AccessibilityEngine();
        var result = await engine.AuditFileAsync(filePath);

        if (result.Issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No accessibility violations found. Nothing to fix![/]");
            return 0;
        }

        var fixes = await RemediationEngine.ApplyFixesToFileAsync(filePath, result);
        if (fixes > 0)
        {
            AnsiConsole.MarkupLine($"[bold green]✔ Successfully applied {fixes} automatic accessibility fixes to {filePath}![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No automatic safe fixes could be applied directly. Manual remediation required for remaining items.[/]");
        }

        return 0;
    }
}
