using System.Text;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Reporters;

public class MarkdownReporter : IAuditReporter
{
    public Task<string> GenerateReportAsync(AuditResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# ♿ Accessibility Agent Audit Report");
        sb.AppendLine();
        sb.AppendLine($"**Target:** `{result.Target}`  ");
        sb.AppendLine($"**Audited:** {result.Timestamp:yyyy-MM-dd HH:mm:ss UTC} | **Duration:** {result.Duration.TotalMilliseconds:F0}ms  ");
        sb.AppendLine($"**Overall Score:** `{result.AccessibilityScore}/100` (Grade: **{result.Grade}**)  ");
        sb.AppendLine();

        // Compliance status badges
        var aaStatus = result.PassesWcagAa ? "✅ PASS" : "❌ FAIL";
        var aaaStatus = result.PassesWcagAaa ? "✅ PASS" : "⚠️ NEEDS REVIEW";
        sb.AppendLine($"| Standard | Status | Criteria Tested |");
        sb.AppendLine($"| :--- | :--- | :--- |");
        sb.AppendLine($"| **WCAG 2.1 / 2.2 AA** | {aaStatus} | Level A & AA |");
        sb.AppendLine($"| **WCAG 2.1 / 2.2 AAA** | {aaaStatus} | Level AAA |");
        sb.AppendLine();

        // Metric breakdown
        sb.AppendLine($"### 📊 Issue Breakdown");
        sb.AppendLine($"| 🔴 Critical | 🟠 Serious | 🟡 Moderate | 🔵 Minor | Total Elements |");
        sb.AppendLine($"| :---: | :---: | :---: | :---: | :---: |");
        sb.AppendLine($"| {result.CriticalCount} | {result.SeriousCount} | {result.ModerateCount} | {result.MinorCount} | {result.ElementsEvaluated} |");
        sb.AppendLine();

        if (result.Issues.Count == 0)
        {
            sb.AppendLine("### 🎉 No accessibility violations detected!");
            sb.AppendLine("All evaluated elements passed the configured accessibility and WCAG criteria.");
            return Task.FromResult(sb.ToString());
        }

        sb.AppendLine("### 🔍 Detailed Reviewer Findings & Remediation");
        sb.AppendLine();

        int index = 1;
        foreach (var issue in result.Issues)
        {
            var severityIcon = issue.Severity switch
            {
                IssueSeverity.Critical => "🔴 **CRITICAL**",
                IssueSeverity.Serious => "🟠 **SERIOUS**",
                IssueSeverity.Moderate => "🟡 **MODERATE**",
                _ => "🔵 **MINOR**"
            };

            sb.AppendLine($"#### {index++}. [{issue.RuleId}] {issue.RuleName}");
            sb.AppendLine($"- **Severity:** {severityIcon}");
            sb.AppendLine($"- **WCAG Criterion:** [{issue.Criterion.Number} {issue.Criterion.Name} ({issue.Criterion.Level})]({issue.Criterion.HelpUrl})");
            sb.AppendLine($"- **Location:** `{issue.FormattedLocation}`");
            sb.AppendLine($"- **Selector:** `{issue.Selector}`");
            sb.AppendLine();
            sb.AppendLine($"> **Issue:** {issue.Message}");
            sb.AppendLine();
            sb.AppendLine($"> **Remediation:** {issue.RemediationSummary}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(issue.HtmlSnippet))
            {
                sb.AppendLine("<details>");
                sb.AppendLine("<summary><b>View Code Snippet & Suggested Fix</b></summary>");
                sb.AppendLine();
                sb.AppendLine("```html");
                sb.AppendLine($"<!-- Current Markup -->");
                sb.AppendLine(issue.HtmlSnippet);
                sb.AppendLine("```");

                if (!string.IsNullOrEmpty(issue.SuggestedFixCode))
                {
                    sb.AppendLine("```diff");
                    sb.AppendLine($"<!-- Suggested Remediation -->");
                    sb.AppendLine($"- {issue.HtmlSnippet}");
                    sb.AppendLine($"+ {issue.SuggestedFixCode}");
                    sb.AppendLine("```");
                }
                sb.AppendLine("</details>");
                sb.AppendLine();
            }
        }

        sb.AppendLine("---");
        sb.AppendLine("### 📋 Reviewer Accessibility Checklist");
        sb.AppendLine("- [ ] Verify all interactive elements have visible focus indicators (`:focus-visible`).");
        sb.AppendLine("- [ ] Test complete page flow using keyboard navigation only (Tab, Shift+Tab, Enter, Space).");
        sb.AppendLine("- [ ] Validate screen reader announcements using NVDA, VoiceOver, or Orca.");
        sb.AppendLine("- [ ] Check color contrast with dark and high-contrast system modes.");
        sb.AppendLine();
        sb.AppendLine("*Generated automatically by Accessibility Agent for Developers and Reviewers.*");

        return Task.FromResult(sb.ToString());
    }
}
