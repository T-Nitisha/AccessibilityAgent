using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Keyboard;

public class TabIndexPositiveRule : BaseRule
{
    public override string Id => "tabindex-positive";
    public override string Name => "Positive tabindex values (> 0) should not be used";
    public override RuleCategory Category => RuleCategory.Keyboard;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.FocusOrder;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elements = document.QuerySelectorAll("[tabindex]");

        foreach (var el in elements)
        {
            var tabIndexStr = el.GetAttribute("tabindex");
            if (int.TryParse(tabIndexStr, out var ti) && ti > 0)
            {
                yield return CreateIssue(
                    el, rawSource,
                    $"Element has tabindex='{ti}'. Positive tabindex values disrupt logical tab flow for keyboard users.",
                    "Use tabindex='0' to insert an element into natural tab order, or tabindex='-1' for programmatic focus. Do not use positive numbers.",
                    el.OuterHtml.Replace($"tabindex=\"{ti}\"", "tabindex=\"0\"")
                );
            }
        }
    }
}
