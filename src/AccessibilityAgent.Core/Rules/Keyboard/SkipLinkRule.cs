using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Keyboard;

public class SkipLinkRule : BaseRule
{
    public override string Id => "skip-link";
    public override string Name => "Pages with repeated navigation should provide a skip to content link";
    public override RuleCategory Category => RuleCategory.Keyboard;
    public override IssueSeverity Severity => IssueSeverity.Moderate;
    public override WcagCriterion Criterion => WcagCriterion.BypassBlocks;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        if (!rawSource.Contains("<body", StringComparison.OrdinalIgnoreCase))
            yield break;

        // Check if page has header or nav
        var hasNav = document.QuerySelector("header, nav") != null;
        if (!hasNav) yield break;

        // Check for skip link
        var firstLinks = document.QuerySelectorAll("body a").Take(3);
        bool hasSkipLink = firstLinks.Any(l => 
            (l.GetAttribute("href")?.StartsWith('#') ?? false) && 
            (l.TextContent?.Contains("skip", StringComparison.OrdinalIgnoreCase) ?? false));

        if (!hasSkipLink && document.Body != null)
        {
            yield return CreateIssue(
                document.Body, rawSource,
                "Page has navigation but lacks a 'Skip to main content' link as the first focusable element in the body.",
                "Add an accessible skip link at the top of <body>, e.g. <a href='#main-content' class='sr-only-focusable'>Skip to main content</a>.",
                "<a href=\"#main-content\" class=\"skip-link\">Skip to main content</a>"
            );
        }
    }
}
