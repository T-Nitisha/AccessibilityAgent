using System.Text;
using AngleSharp.Dom;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules;

public abstract class BaseRule : IAccessibilityRule
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract RuleCategory Category { get; }
    public abstract IssueSeverity Severity { get; }
    public abstract WcagCriterion Criterion { get; }

    public abstract IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource);

    protected AccessibilityIssue CreateIssue(
        IElement element,
        string rawSource,
        string message,
        string remediation,
        string? suggestedFix = null,
        IssueSeverity? overrideSeverity = null)
    {
        var (line, col) = SourceLocationFinder.FindLocation(rawSource, element);
        var selector = GetCssSelector(element);
        var snippet = GetOuterHtmlSnippet(element);

        return new AccessibilityIssue(
            Id: $"{Id}-{Guid.NewGuid().ToString("N")[..6]}",
            RuleId: Id,
            RuleName: Name,
            Severity: overrideSeverity ?? Severity,
            Message: message,
            Selector: selector,
            HtmlSnippet: snippet,
            LineNumber: line,
            ColumnNumber: col,
            Criterion: Criterion,
            RemediationSummary: remediation,
            SuggestedFixCode: suggestedFix
        );
    }

    protected static string GetCssSelector(IElement element)
    {
        if (!string.IsNullOrEmpty(element.Id))
            return $"#{element.Id}";

        var sb = new StringBuilder();
        var current = element;

        while (current != null && current.NodeName.ToLowerInvariant() != "html" && current.NodeName.ToLowerInvariant() != "#document")
        {
            var tag = current.TagName.ToLowerInvariant();
            if (!string.IsNullOrEmpty(current.Id))
            {
                sb.Insert(0, $"#{current.Id} > ");
                break;
            }

            var className = current.ClassName;
            if (!string.IsNullOrEmpty(className))
            {
                var firstClass = className.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(firstClass))
                {
                    tag += $".{firstClass}";
                }
            }

            // Find index among same-tag siblings
            if (current.ParentElement != null)
            {
                var siblings = current.ParentElement.Children
                    .Where(c => c.TagName.Equals(current.TagName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (siblings.Count > 1)
                {
                    var index = siblings.IndexOf(current) + 1;
                    tag += $":nth-of-type({index})";
                }
            }

            sb.Insert(0, sb.Length > 0 ? $"{tag} > " : tag);
            current = current.ParentElement;
        }

        return sb.ToString().TrimEnd(' ', '>');
    }

    protected static string GetOuterHtmlSnippet(IElement element, int maxLength = 140)
    {
        var html = element.OuterHtml;
        if (string.IsNullOrEmpty(html))
            return $"<{element.TagName.ToLowerInvariant()}>";

        // Normalize whitespace
        var normalized = string.Join(" ", html.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length <= maxLength)
            return normalized;

        return normalized.Substring(0, maxLength - 3) + "...";
    }
}
