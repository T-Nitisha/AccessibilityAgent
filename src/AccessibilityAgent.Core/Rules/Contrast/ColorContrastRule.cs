using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Contrast;

public class ColorContrastRule : BaseRule
{
    private static readonly Regex ColorRegex = new(@"(?<![\w-])color\s*:\s*([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex BgColorRegex = new(@"(?<![\w-])background(-color)?\s*:\s*([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override string Id => "color-contrast";
    public override string Name => "Text and background colors must have sufficient contrast (>= 4.5:1)";
    public override RuleCategory Category => RuleCategory.Contrast;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.ContrastMinimum;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elements = document.QuerySelectorAll("[style*='color'], [style*='background']");

        foreach (var el in elements)
        {
            var text = el.TextContent?.Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;

            var style = el.GetAttribute("style") ?? "";
            var colorMatch = ColorRegex.Match(style);
            var bgMatch = BgColorRegex.Match(style);

            string? fgColor = colorMatch.Success ? colorMatch.Groups[1].Value.Trim() : null;
            string? bgColor = bgMatch.Success ? bgMatch.Groups[2].Value.Trim() : null;

            // If only foreground is set, check parent's background
            if (fgColor != null && bgColor == null)
            {
                var parentWithBg = el.ParentElement;
                while (parentWithBg != null)
                {
                    var parentStyle = parentWithBg.GetAttribute("style") ?? "";
                    var pBgMatch = BgColorRegex.Match(parentStyle);
                    if (pBgMatch.Success)
                    {
                        bgColor = pBgMatch.Groups[2].Value.Trim();
                        break;
                    }
                    parentWithBg = parentWithBg.ParentElement;
                }
            }

            if (fgColor != null && bgColor != null)
            {
                // Clean color strings
                fgColor = fgColor.Split('!')[0].Trim();
                bgColor = bgColor.Split('!')[0].Trim();

                var contrast = ContrastCalculator.Calculate(fgColor, bgColor);

                // Check font-size to see if it qualifies as large text (>= 18pt or >= 14pt bold, approx 24px or 18.66px bold)
                bool isLarge = style.Contains("font-size: 24px") || style.Contains("font-size:24px") ||
                               style.Contains("font-size: 2rem") || style.Contains("font-size:2rem") ||
                               el.TagName.ToLowerInvariant() is "h1" or "h2";

                bool passes = isLarge ? contrast.PassesLargeAa : contrast.PassesNormalAa;
                double targetRatio = isLarge ? 3.0 : 4.5;

                if (!passes)
                {
                    yield return CreateIssue(
                        el, rawSource,
                        $"Insufficient color contrast: ratio is {contrast.Ratio}:1 (Foreground: {fgColor}, Background: {bgColor}), minimum required is {targetRatio}:1.",
                        $"Adjust the text color or background color to achieve a contrast ratio of at least {targetRatio}:1 (WCAG AA).",
                        null
                    );
                }
            }
        }
    }
}
