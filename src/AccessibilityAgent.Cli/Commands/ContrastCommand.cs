using Spectre.Console;
using AccessibilityAgent.Core.Analysis;

namespace AccessibilityAgent.Cli.Commands;

public static class ContrastCommand
{
    public static int Execute(string foreground, string background)
    {
        var result = ContrastCalculator.Calculate(foreground, background);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold yellow]🎨 Color Contrast Ratio Analysis[/]") { Justification = Justify.Left });
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Parameter");
        table.AddColumn("Value");

        table.AddRow("Foreground", foreground);
        table.AddRow("Background", background);
        table.AddRow("Contrast Ratio", $"[bold {GetRatioColor(result.Ratio)}]{result.Ratio}:1[/]");
        table.AddRow("Compliance Rating", $"[bold]{result.RatingString}[/]");
        table.AddRow("WCAG AA Normal Text (min 4.5:1)", result.PassesNormalAa ? "[green]PASS[/]" : "[red]FAIL[/]");
        table.AddRow("WCAG AA Large Text (min 3.0:1)", result.PassesLargeAa ? "[green]PASS[/]" : "[red]FAIL[/]");
        table.AddRow("WCAG AAA Normal Text (min 7.0:1)", result.PassesNormalAaa ? "[green]PASS[/]" : "[red]FAIL[/]");
        table.AddRow("WCAG AAA Large Text (min 4.5:1)", result.PassesLargeAaa ? "[green]PASS[/]" : "[red]FAIL[/]");
        table.AddRow("UI Components & Graphics (min 3.0:1)", result.PassesUiComponent ? "[green]PASS[/]" : "[red]FAIL[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        return result.PassesNormalAa ? 0 : 1;
    }

    private static string GetRatioColor(double ratio) => ratio switch
    {
        >= 7.0 => "green",
        >= 4.5 => "blue",
        >= 3.0 => "yellow",
        _ => "red"
    };
}
