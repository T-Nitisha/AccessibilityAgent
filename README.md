# ♿ Accessibility Agent (.NET 8)

An enterprise-grade, developer- and reviewer-focused **Accessibility Testing & WCAG Compliance Agent** built in C# (.NET 8).

Accessibility Agent enables engineers and PR reviewers to automatically audit web markup, Blazor/Razor components, JSX/TSX templates, and live URLs against **WCAG 2.1 & 2.2 (Level A, AA, AAA)** and **Section 508** standards.

---

## ✨ Features

- **⚡ Blazing Fast DOM & Markup Analysis**: Powered by AngleSharp for Living Standard HTML5 parsing without requiring heavy browser binaries.
- **🎯 20+ Built-in WCAG Rules**:
  - **Images & Non-text (1.1.1)**: Missing alt attributes, decorative role validation, placeholder/filename detection.
  - **Structure & Semantics (1.3.1, 2.4.1, 2.4.2, 3.1.1)**: `lang` attribute, `<title>`, heading hierarchy skips (`<h1>` &rarr; `<h3>`), table headers (`<th>` with `scope`), list structures, `<main>` landmarks, and skip links.
  - **Forms & Inputs (1.3.5, 3.3.2, 4.1.2)**: Accessible labels (`<label for>`, `aria-label`), `<fieldset>`/`<legend>` for radio/checkbox groups, autocomplete identity hints.
  - **Navigation & Links (2.4.4, 4.1.2)**: Non-empty buttons, ambiguous link text ("click here", "read more"), `target="_blank"` security & screen reader warnings.
  - **Keyboard & Focus (2.1.1, 2.4.3, 2.4.7)**: Clickable `<div>`/`<span>` without `tabindex`, disruptive positive `tabindex` (> 0), focus outline suppression (`outline: none`).
  - **ARIA & Accessibility Object Model (4.1.2)**: Valid W3C ARIA roles and attributes, duplicate IDs, and detection of focusable elements trapped inside `aria-hidden="true"`.
  - **Color Contrast (1.4.3, 1.4.6, 1.4.11)**: Exact IEC 61966-2-1 relative luminance contrast algorithm for normal text (4.5:1), large text (3:1), AAA (7:1), and UI components.
  - **Media (1.2.2, 1.4.2)**: Missing `<track kind="captions">` on `<video>` and unmuted autoplay detection.
- **🧑‍💻 Reviewer PR Mode (`review`)**:
  - File-by-file and line-by-line inspection with exact file paths and line numbers.
  - Auto-generated GitHub/GitLab Pull Request comment markdown (`--comment-output`).
  - Unified code diff patches (`--patch-output`).
- **🛠️ Auto-Fix Remediation (`fix`)**:
  - Automatically remediates simple violations directly in source files.
- **🌳 Accessibility Object Model (AOM) Tree (`tree`)**:
  - Visual terminal tree representing what screen readers (NVDA, VoiceOver, JAWS) announce.
- **🎨 Interactive Web Reviewer Studio (`serve`)**:
  - Embedded ASP.NET Core web interface on `http://localhost:5050` with live code testing, URL auditing, and contrast calculator.
- **📊 Standard Output Formats**:
  - Rich Spectre.Console terminal UI.
  - Interactive Standalone HTML Dashboard with severity filtering and theme support.
  - SARIF (Static Analysis Results Interchange Format v2.1.0) for GitHub Code Scanning / Security tab.
  - Markdown for PR comments.

---

## 🚀 Quick Start

### 1. Requirements
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2. Build the Solution
```bash
dotnet build
```

### 3. Run Unit Tests
```bash
dotnet test
```

---

## 📖 CLI Usage

Run commands via `dotnet run --project src/AccessibilityAgent.Cli -- <command> [options]`.

### 1. Audit a File, Directory, or Live URL
```bash
# Audit an HTML or Razor template
dotnet run --project src/AccessibilityAgent.Cli -- audit examples/inaccessible-page.html

# Audit a live website
dotnet run --project src/AccessibilityAgent.Cli -- audit https://example.com

# Generate a standalone interactive HTML report
dotnet run --project src/AccessibilityAgent.Cli -- audit examples/inaccessible-page.html --format html --output report.html

# Generate SARIF report for GitHub Code Scanning
dotnet run --project src/AccessibilityAgent.Cli -- audit examples/inaccessible-page.html --format sarif --output a11y.sarif
```

### 2. PR Review Mode (For Reviewers)
```bash
# Review an entire directory or PR changes
dotnet run --project src/AccessibilityAgent.Cli -- review ./Views --comment-output pr-comment.md --patch-output pr-fixes.patch
```

### 3. Inspect the Accessibility Tree (AOM)
```bash
dotnet run --project src/AccessibilityAgent.Cli -- tree examples/accessible-page.html
```

### 4. Fast Color Contrast Checker
```bash
dotnet run --project src/AccessibilityAgent.Cli -- contrast "#4f46e5" "#ffffff"
```

### 5. Launch Interactive Reviewer Web Studio
```bash
dotnet run --project src/AccessibilityAgent.Cli -- serve --port 5050
```
Open [http://localhost:5050](http://localhost:5050) in your browser to test snippets, URLs, and contrast interactively.

### 6. Auto-Remediate Violations
```bash
dotnet run --project src/AccessibilityAgent.Cli -- fix index.html
```

---

## 🔄 GitHub Actions CI/CD Integration

The agent integrates directly into GitHub Pull Request checks (`.github/workflows/accessibility-audit.yml`):

```yaml
name: Accessibility PR Audit

on:
  pull_request:
    branches: [ main ]

jobs:
  a11y-audit:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Run Accessibility Agent
        run: |
          dotnet run --project src/AccessibilityAgent.Cli -- review . --comment-output a11y-review.md
```

---

## 🏗️ Project Architecture

```
AccessibilityAgent/
├── src/
│   ├── AccessibilityAgent.Core/       # Domain models, IEC luminance contrast calculator,
│   │                                  # Accessibility tree builder, 20+ WCAG rules,
│   │                                  # HTML/Markdown/SARIF reporters
│   └── AccessibilityAgent.Cli/        # Spectre.Console CLI, commands, and embedded
│                                      # ASP.NET Core Reviewer Studio web server
├── tests/
│   └── AccessibilityAgent.Tests/      # xUnit test suite (100% passing tests)
├── examples/
│   ├── inaccessible-page.html         # Inaccessible baseline sample with 23+ violations
│   ├── accessible-page.html           # 100/100 A+ fully remediated WCAG reference
│   └── UserCard.razor                 # Blazor/Razor component accessibility sample
```

---

## 👤 Author
Authored by [T-Nitisha](https://github.com/T-Nitisha).

## ⚖️ License
MIT
