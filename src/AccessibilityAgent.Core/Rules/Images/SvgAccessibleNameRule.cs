using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Images;

public class SvgAccessibleNameRule : BaseRule
{
    public override string Id => "svg-accessible-name";
    public override string Name => "SVGs must have an accessible name or be hidden";
    public override RuleCategory Category => RuleCategory.Images;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.NonTextContent;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var svgs = document.QuerySelectorAll("svg");

        foreach (var svg in svgs)
        {
            // If svg is hidden or marked aria-hidden="true", it is fine
            if (svg.GetAttribute("aria-hidden") == "true")
                continue;

            // If inside a button or link with accessible text, it is fine
            var parentInteractive = svg.Closest("button, a");
            if (parentInteractive != null)
            {
                var parentText = parentInteractive.TextContent?.Trim();
                var parentAria = parentInteractive.GetAttribute("aria-label") ?? parentInteractive.GetAttribute("aria-labelledby");
                if (!string.IsNullOrWhiteSpace(parentText) || !string.IsNullOrWhiteSpace(parentAria))
                {
                    // Parent has label, so svg is considered decorative or icon
                    continue;
                }
            }

            // Check if SVG has title child, aria-label, or aria-labelledby
            var titleElem = svg.QuerySelector("title");
            var hasTitle = titleElem != null && !string.IsNullOrWhiteSpace(titleElem.TextContent);
            var hasAriaLabel = !string.IsNullOrWhiteSpace(svg.GetAttribute("aria-label"));
            var hasAriaLabelledBy = !string.IsNullOrWhiteSpace(svg.GetAttribute("aria-labelledby"));

            if (!hasTitle && !hasAriaLabel && !hasAriaLabelledBy)
            {
                yield return CreateIssue(
                    svg, rawSource,
                    "SVG element has no accessible name (missing <title>, aria-label, or aria-labelledby).",
                    "Add an <title> element inside the SVG or an aria-label attribute. If purely decorative, add aria-hidden=\"true\".",
                    svg.OuterHtml.Replace("<svg", "<svg aria-hidden=\"true\"")
                );
            }
        }
    }
}
