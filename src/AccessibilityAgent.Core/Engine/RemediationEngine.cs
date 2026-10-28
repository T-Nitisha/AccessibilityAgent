using System.Text;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Engine;

public static class RemediationEngine
{
    public static string GenerateReviewPatch(AuditResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- a/{result.Target}");
        sb.AppendLine($"+++ b/{result.Target}");

        foreach (var issue in result.Issues.Where(i => !string.IsNullOrEmpty(i.SuggestedFixCode)))
        {
            sb.AppendLine($"@@ -{issue.LineNumber},1 +{issue.LineNumber},1 @@ [{issue.RuleId}] {issue.Message}");
            sb.AppendLine($"- {issue.HtmlSnippet}");
            sb.AppendLine($"+ {issue.SuggestedFixCode}");
        }

        return sb.ToString();
    }

    public static async Task<int> ApplyFixesToFileAsync(string filePath, AuditResult result)
    {
        if (!File.Exists(filePath))
            return 0;

        var content = await File.ReadAllTextAsync(filePath);
        int fixesApplied = 0;

        foreach (var issue in result.Issues)
        {
            if (!string.IsNullOrEmpty(issue.SuggestedFixCode) && !string.IsNullOrEmpty(issue.HtmlSnippet))
            {
                if (content.Contains(issue.HtmlSnippet))
                {
                    content = content.Replace(issue.HtmlSnippet, issue.SuggestedFixCode);
                    fixesApplied++;
                }
            }
        }

        if (fixesApplied > 0)
        {
            await File.WriteAllTextAsync(filePath, content);
        }

        return fixesApplied;
    }
}
