using AngleSharp.Dom;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Navigation;

public class ButtonNameRule : BaseRule
{
    public override string Id => "button-name";
    public override string Name => "Buttons must have an accessible text name";
    public override RuleCategory Category => RuleCategory.Navigation;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.NameRoleValue;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var buttons = document.QuerySelectorAll("button, [role='button'], input[type='button'], input[type='submit'], input[type='reset']");

        foreach (var button in buttons)
        {
            var accessibleName = AccessibilityTreeBuilder.ComputeAccessibleName(button);

            if (string.IsNullOrWhiteSpace(accessibleName))
            {
                yield return CreateIssue(
                    button, rawSource,
                    "Button has no discernible accessible text. Screen readers will announce it as an unlabeled button.",
                    "Add visible text content, an aria-label attribute, or aria-labelledby pointing to descriptive text.",
                    button.OuterHtml.Replace("<button", "<button aria-label=\"Action description\"")
                );
            }
        }
    }
}
