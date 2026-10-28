using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Keyboard;

public class KeyboardNavigableRule : BaseRule
{
    private static readonly HashSet<string> NonInteractiveTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "div", "span", "p", "section", "article", "header", "footer", "aside", "li", "tr", "td"
    };

    public override string Id => "keyboard-navigable";
    public override string Name => "Interactive elements must be keyboard focusable and operable";
    public override RuleCategory Category => RuleCategory.Keyboard;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.Keyboard;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        // Find non-interactive tags with click handlers or button role
        var elements = document.QuerySelectorAll("div, span, p, li, td, tr, section");

        foreach (var el in elements)
        {
            var tag = el.TagName.ToLowerInvariant();
            var hasOnClick = el.HasAttribute("onclick") || el.HasAttribute("@onclick");
            var role = el.GetAttribute("role")?.ToLowerInvariant();
            var hasButtonRole = role is "button" or "link" or "menuitem";

            if (hasOnClick || hasButtonRole)
            {
                var tabIndex = el.GetAttribute("tabindex");
                var hasValidTabIndex = int.TryParse(tabIndex, out var ti) && ti >= 0;

                if (!hasValidTabIndex)
                {
                    yield return CreateIssue(
                        el, rawSource,
                        $"Non-interactive <{tag}> has click interaction or role='{role}' but is not keyboard focusable (missing tabindex='0').",
                        "Prefer using native <button> or <a> tags. If using custom elements, add tabindex='0' and implement onkeydown / onkeyup handlers (Enter and Space keys).",
                        $"<button type=\"button\">{el.InnerHtml}</button>"
                    );
                }
            }
        }
    }
}
