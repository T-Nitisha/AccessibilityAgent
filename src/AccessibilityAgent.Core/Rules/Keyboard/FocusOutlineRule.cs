using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Keyboard;

public class FocusOutlineRule : BaseRule
{
    public override string Id => "focus-outline";
    public override string Name => "Focus outlines must not be removed without alternative visible indicators";
    public override RuleCategory Category => RuleCategory.Keyboard;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.FocusVisible;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elements = document.QuerySelectorAll("[style*='outline']");

        foreach (var el in elements)
        {
            var style = el.GetAttribute("style")?.ToLowerInvariant() ?? "";
            if (style.Contains("outline:none") || style.Contains("outline: none") || 
                style.Contains("outline:0") || style.Contains("outline: 0"))
            {
                yield return CreateIssue(
                    el, rawSource,
                    "Element explicitly suppresses keyboard focus outline (outline: none / 0). Keyboard users cannot tell which element is focused.",
                    "Retain native focus ring or define a high-contrast custom focus indicator using CSS :focus-visible.",
                    el.OuterHtml.Replace("outline:none;", "").Replace("outline: none;", "")
                );
            }
        }
    }
}
