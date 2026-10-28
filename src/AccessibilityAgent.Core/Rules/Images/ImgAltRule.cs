using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Images;

public class ImgAltRule : BaseRule
{
    private static readonly Regex SuspiciousAltRegex = new(
        @"^(\d+|\.(jpg|png|gif|svg|webp)|image|photo|picture|pic|icon|graphic|banner|placeholder|img_\d+)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override string Id => "image-alt";
    public override string Name => "Images must have an alternative text (alt)";
    public override RuleCategory Category => RuleCategory.Images;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.NonTextContent;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var images = document.QuerySelectorAll("img");

        foreach (var img in images)
        {
            var role = img.GetAttribute("role")?.ToLowerInvariant();
            bool isDecorative = role is "presentation" or "none";

            if (!img.HasAttribute("alt"))
            {
                if (isDecorative)
                {
                    yield return CreateIssue(
                        img, rawSource,
                        "Image marked as decorative with role='presentation' is still missing an explicit empty alt='' attribute.",
                        "Add alt='' to explicitly convey to assistive tech that the image is decorative.",
                        img.OuterHtml.Replace("<img", "<img alt=\"\"")
                    );
                }
                else
                {
                    yield return CreateIssue(
                        img, rawSource,
                        "Image is missing an 'alt' attribute, leaving screen reader users without context.",
                        "Provide a concise, meaningful alt text describing the image content or purpose. If purely decorative, use alt=\"\".",
                        img.OuterHtml.Replace("<img", "<img alt=\"Descriptive image text\"")
                    );
                }
                continue;
            }

            var altText = img.GetAttribute("alt") ?? "";

            // If not decorative, check for suspicious or file-extension alt texts
            if (!isDecorative && !string.IsNullOrWhiteSpace(altText))
            {
                var trimmed = altText.Trim();
                if (SuspiciousAltRegex.IsMatch(trimmed) || trimmed.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || trimmed.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    yield return CreateIssue(
                        img, rawSource,
                        $"Image alt text '{trimmed}' appears to be a file name, placeholder, or generic label rather than a meaningful description.",
                        "Replace generic or filename alt text with a meaningful description of what the image portrays in context.",
                        img.OuterHtml.Replace($"alt=\"{altText}\"", "alt=\"Detailed description of content\""),
                        IssueSeverity.Moderate
                    );
                }
            }
        }
    }
}
