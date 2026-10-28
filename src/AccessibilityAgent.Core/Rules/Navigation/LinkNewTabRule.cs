using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Navigation;

public class LinkNewTabRule : BaseRule
{
    public override string Id => "link-new-tab";
    public override string Name => "Links opening in new tabs must notify users and include security attributes";
    public override RuleCategory Category => RuleCategory.Navigation;
    public override IssueSeverity Severity => IssueSeverity.Minor;
    public override WcagCriterion Criterion => WcagCriterion.LinkPurpose;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var links = document.QuerySelectorAll("a[target='_blank']");

        foreach (var link in links)
        {
            var rel = link.GetAttribute("rel") ?? "";
            bool hasNoOpener = rel.Contains("noopener", StringComparison.OrdinalIgnoreCase);

            var text = link.TextContent ?? "";
            var ariaLabel = link.GetAttribute("aria-label") ?? "";
            bool hasNewTabWarning = text.Contains("new window", StringComparison.OrdinalIgnoreCase) ||
                                    text.Contains("new tab", StringComparison.OrdinalIgnoreCase) ||
                                    ariaLabel.Contains("new window", StringComparison.OrdinalIgnoreCase) ||
                                    ariaLabel.Contains("new tab", StringComparison.OrdinalIgnoreCase);

            if (!hasNoOpener)
            {
                yield return CreateIssue(
                    link, rawSource,
                    "Link opens in a new tab (target='_blank') but is missing rel='noopener noreferrer', posing security and performance risks.",
                    "Add rel='noopener noreferrer' to all target='_blank' links.",
                    link.OuterHtml.Replace("<a", "<a rel=\"noopener noreferrer\"")
                );
            }

            if (!hasNewTabWarning)
            {
                yield return CreateIssue(
                    link, rawSource,
                    "Link opens in a new tab without notifying assistive technology users.",
                    "Include visual or screen-reader indication such as '(opens in a new tab)' or add it to aria-label.",
                    link.OuterHtml.Replace("<a", "<a aria-label=\"Link destination (opens in a new tab)\""),
                    IssueSeverity.Minor
                );
            }
        }
    }
}
