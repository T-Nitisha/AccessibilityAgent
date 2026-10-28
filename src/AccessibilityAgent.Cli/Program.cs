using Spectre.Console;
using AccessibilityAgent.Cli.Commands;
using AccessibilityAgent.Cli.Web;

namespace AccessibilityAgent.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
        {
            ShowHelp();
            return 0;
        }

        if (args[0] is "--version" or "-v")
        {
            AnsiConsole.MarkupLine("[bold cyan]AccessibilityAgent v1.0.0[/] (.NET 8)");
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var remaining = args.Skip(1).ToArray();

        try
        {
            return command switch
            {
                "audit" => await HandleAuditAsync(remaining),
                "review" => await HandleReviewAsync(remaining),
                "fix" => await HandleFixAsync(remaining),
                "tree" => await HandleTreeAsync(remaining),
                "contrast" => HandleContrast(remaining),
                "serve" => await HandleServeAsync(remaining),
                "init" => await InitCommand.ExecuteAsync(),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Fatal error:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }

    private static void ShowHelp()
    {
        AnsiConsole.Write(new FigletText("A11y Agent").Color(Color.Yellow));
        AnsiConsole.MarkupLine("[bold cyan]Accessibility Agent for Developers & Reviewers[/] - WCAG 2.1 & 2.2 Auditor\n");

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold]Command[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Usage Example[/]");

        table.AddRow("audit", "Audit a URL, HTML file, or directory", "a11y-agent audit index.html --format html -o report.html");
        table.AddRow("review", "Code Review / PR mode with line numbers and diffs", "a11y-agent review ./Views --comment-output review.md");
        table.AddRow("fix", "Automatically apply safe accessibility fixes", "a11y-agent fix index.html");
        table.AddRow("tree", "Inspect the computed Accessibility Object Tree", "a11y-agent tree https://example.com");
        table.AddRow("contrast", "Analyze color contrast against WCAG thresholds", "a11y-agent contrast \"#3b82f6\" \"#ffffff\"");
        table.AddRow("serve", "Start interactive Web Reviewer Studio", "a11y-agent serve --port 5050");
        table.AddRow("init", "Generate config, CI/CD workflow & reviewer agent", "a11y-agent init");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Flags: --format <cli|html|markdown|sarif>, --output <path>, --level <A|AA|AAA>, --fail-on <critical|serious|moderate|minor>[/]\n");
    }

    private static async Task<int> HandleAuditAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: Target URL or file path is required for audit.[/]");
            AnsiConsole.MarkupLine("Usage: a11y-agent audit <url|file|dir> [--format cli|html|markdown|sarif] [--output file] [--level A|AA|AAA]");
            return 1;
        }

        string target = args[0];
        string format = GetOption(args, "--format", "-f") ?? "cli";
        string? output = GetOption(args, "--output", "-o");
        string level = GetOption(args, "--level", "-l") ?? "AA";
        string failOn = GetOption(args, "--fail-on") ?? "critical";

        return await AuditCommand.ExecuteAsync(target, format, output, level, failOn);
    }

    private static async Task<int> HandleReviewAsync(string[] args)
    {
        string path = args.Length > 0 && !args[0].StartsWith("-") ? args[0] : ".";
        string? commentOutput = GetOption(args, "--comment-output", "-c");
        string? patchOutput = GetOption(args, "--patch-output", "-p");

        return await ReviewCommand.ExecuteAsync(path, commentOutput, patchOutput);
    }

    private static async Task<int> HandleFixAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: Target file required for fix command.[/]");
            return 1;
        }
        return await FixCommand.ExecuteAsync(args[0]);
    }

    private static async Task<int> HandleTreeAsync(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Error: Target file or URL required for tree command.[/]");
            return 1;
        }
        return await TreeCommand.ExecuteAsync(args[0]);
    }

    private static int HandleContrast(string[] args)
    {
        if (args.Length < 2)
        {
            AnsiConsole.MarkupLine("[red]Error: Both foreground and background color required.[/]");
            AnsiConsole.MarkupLine("Usage: a11y-agent contrast \"#3b82f6\" \"#ffffff\"");
            return 1;
        }
        return ContrastCommand.Execute(args[0], args[1]);
    }

    private static async Task<int> HandleServeAsync(string[] args)
    {
        var portStr = GetOption(args, "--port", "-p") ?? "5050";
        int port = int.TryParse(portStr, out int p) ? p : 5050;

        AnsiConsole.MarkupLine($"[bold green]🚀 Starting Accessibility Reviewer Studio at http://localhost:{port}[/]");
        AnsiConsole.MarkupLine("[dim]Press Ctrl+C to stop the server[/]\n");

        await WebStudioServer.StartAsync(port);
        return 0;
    }

    private static int HandleUnknownCommand(string cmd)
    {
        AnsiConsole.MarkupLine($"[red]Unknown command: {cmd}[/]");
        AnsiConsole.MarkupLine("Run [yellow]a11y-agent --help[/] for available commands.");
        return 1;
    }

    private static string? GetOption(string[] args, params string[] names)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (names.Contains(args[i], StringComparer.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
