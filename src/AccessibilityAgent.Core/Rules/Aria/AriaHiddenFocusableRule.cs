using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Aria;

public class AriaHiddenFocusableRule : BaseRule
{
    public override string Id => "aria-hidden-focusable";
    public override string Name => "aria-hidden='true' elements must not contain focusable elements";
    public override RuleCategory Category => RuleCategory.Aria;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.NameRoleValue;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var ariaHiddenElements = document.QuerySelectorAll("[aria-hidden='true']");

        foreach (var el in ariaHiddenElements)
        {
            var tag = el.TagName.ToLowerInvariant();
            bool isDirectlyFocusable = tag is "button" or "a" or "input" or "select" or "textarea" ||
                                       (int.TryParse(el.GetAttribute("tabindex"), out var ti) && ti >= 0);

            if (isDirectlyFocusable && !el.HasAttribute("disabled"))
            {
                yield return CreateIssue(
                    el, rawSource,
                    $"Focusable element <{tag}> has aria-hidden='true'. Keyboard focus can reach it, but screen readers will be completely silent.",
                    "Remove aria-hidden='true', or add tabindex='-1' / disabled so it cannot receive keyboard focus.",
                    el.OuterHtml.Replace("aria-hidden=\"true\"", "")
                );
            }
            else
            {
                // Check if any children are focusable
                var focusableChildren = el.QuerySelectorAll("a[href], button:not([disabled]), input:not([disabled]):not([type='hidden']), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex='-1'])");
                if (focusableChildren.Length > 0)
                {
                    yield return CreateIssue(
                        el, rawSource,
                        $"Element with aria-hidden='true' contains {focusableChildren.Length} focusable interactive descendant(s).",
                        "Ensure interactive children have tabindex='-1' or remove aria-hidden='true' from the container.",
                        null
                    );
                }
            }
        }
    }
}
