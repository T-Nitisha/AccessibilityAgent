using Spectre.Console;

namespace AccessibilityAgent.Cli.Commands;

public static class InitCommand
{
    public static async Task<int> ExecuteAsync()
    {
        AnsiConsole.MarkupLine("[bold cyan]🚀 Initializing Accessibility Agent configuration and CI/CD workflows...[/]\n");

        // 1. Create .a11yagentrc.json
        var configJson = @"{
  ""standard"": ""WCAG21AA"",
  ""failOn"": ""critical"",
  ""rules"": {
    ""image-alt"": ""error"",
    ""form-input-label"": ""error"",
    ""button-name"": ""error"",
    ""color-contrast"": ""error"",
    ""html-has-lang"": ""error"",
    ""keyboard-navigable"": ""error"",
    ""tabindex-positive"": ""error"",
    ""aria-valid-role"": ""error"",
    ""aria-hidden-focusable"": ""error""
  },
  ""include"": [
    ""**/*.html"",
    ""**/*.cshtml"",
    ""**/*.razor"",
    ""**/*.jsx"",
    ""**/*.tsx""
  ],
  ""exclude"": [
    ""**/bin/**"",
    ""**/obj/**"",
    ""**/node_modules/**""
  ]
}";
        await File.WriteAllTextAsync(".a11yagentrc.json", configJson);
        AnsiConsole.MarkupLine("[green]✔ Created .a11yagentrc.json[/]");

        // 2. Create GitHub Actions workflow
        Directory.CreateDirectory(".github/workflows");
        var githubWorkflow = @"name: Accessibility PR Audit

on:
  pull_request:
    branches: [ main, master ]
  push:
    branches: [ main, master ]

jobs:
  a11y-audit:
    name: WCAG Accessibility Check
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Run Accessibility Agent Review
        run: |
          dotnet run --project src/AccessibilityAgent.Cli -- review . --comment-output a11y-review.md --patch-output a11y-fixes.patch

      - name: Comment PR with Accessibility Review
        if: github.event_name == 'pull_request' && failure()
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            if (fs.existsSync('a11y-review.md')) {
              const body = fs.readFileSync('a11y-review.md', 'utf8');
              github.rest.issues.createComment({
                issue_number: context.issue.number,
                owner: context.repo.owner,
                repo: context.repo.repo,
                body: body
              });
            }
";
        await File.WriteAllTextAsync(".github/workflows/accessibility-audit.yml", githubWorkflow);
        AnsiConsole.MarkupLine("[green]✔ Created .github/workflows/accessibility-audit.yml[/]");

        AnsiConsole.MarkupLine("\n[bold green]Accessibility Agent initialized successfully![/]");
        return 0;
    }
}
