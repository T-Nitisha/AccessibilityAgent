using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules;

public interface IAccessibilityRule
{
    string Id { get; }
    string Name { get; }
    RuleCategory Category { get; }
    IssueSeverity Severity { get; }
    WcagCriterion Criterion { get; }

    IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource);
}
