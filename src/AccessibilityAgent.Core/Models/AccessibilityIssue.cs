namespace AccessibilityAgent.Core.Models;

public record AccessibilityIssue(
    string Id,
    string RuleId,
    string RuleName,
    IssueSeverity Severity,
    string Message,
    string Selector,
    string HtmlSnippet,
    int LineNumber,
    int ColumnNumber,
    WcagCriterion Criterion,
    string RemediationSummary,
    string? SuggestedFixCode = null,
    string? TargetFile = null)
{
    public string FormattedLocation => TargetFile != null 
        ? $"{TargetFile}:{LineNumber}:{ColumnNumber}"
        : $"Line {LineNumber}, Col {ColumnNumber}";
}
