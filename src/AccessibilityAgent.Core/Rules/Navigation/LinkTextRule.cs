using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Navigation;

public class LinkTextRule : BaseRule
{
    private static readonly Regex AmbiguousLinkText = new(
        @"^(click\s*here|here|read\s*more|more|link|learn\s*more|details|info|view)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override string Id => "link-text";
    public override string Name => "Links must have discernible and descriptive text";
    public override RuleCategory Category => RuleCategory.Navigation;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.LinkPurpose;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var links = document.QuerySelectorAll("a[href]");

        foreach (var link in links)
        {
            var accessibleName = AccessibilityTreeBuilder.ComputeAccessibleName(link);

            if (string.IsNullOrWhiteSpace(accessibleName))
            {
                yield return CreateIssue(
                    link, rawSource,
                    "Link (<a>) has no accessible text. Screen reader users cannot discern the link destination.",
                    "Add descriptive text inside the link, or an aria-label attribute.",
                    link.OuterHtml.Replace("<a", "<a aria-label=\"Descriptive destination\"")
                );
            }
            else if (AmbiguousLinkText.IsMatch(accessibleName.Trim()))
            {
                yield return CreateIssue(
                    link, rawSource,
                    $"Link text '{accessibleName}' is ambiguous and lacks context when read out of order in a screen reader links list.",
                    "Replace vague phrases like 'Click here' or 'Read more' with specific destinations, e.g., 'Read more about Accessibility Guidelines'.",
                    link.OuterHtml.Replace(accessibleName, $"{accessibleName} about this topic"),
                    IssueSeverity.Moderate
                );
            }
        }
    }
}
