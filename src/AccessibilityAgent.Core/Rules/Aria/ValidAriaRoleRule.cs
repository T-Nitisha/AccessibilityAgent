using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Aria;

public class ValidAriaRoleRule : BaseRule
{
    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "alert", "alertdialog", "application", "article", "banner", "blockquote", "button",
        "caption", "cell", "checkbox", "code", "columnheader", "combobox", "complementary",
        "contentinfo", "definition", "deletion", "dialog", "directory", "document", "emphasis",
        "feed", "figure", "form", "generic", "grid", "gridcell", "group", "heading", "img",
        "insertion", "link", "list", "listbox", "listitem", "log", "main", "marquee", "math",
        "meter", "menu", "menubar", "menuitem", "menuitemcheckbox", "menuitemradio", "navigation",
        "none", "note", "option", "paragraph", "presentation", "progressbar", "radio",
        "radiogroup", "region", "row", "rowgroup", "rowheader", "scrollbar", "search",
        "searchbox", "separator", "slider", "spinbutton", "status", "strong", "subscript",
        "superscript", "switch", "tab", "table", "tablist", "tabpanel", "term", "textbox",
        "time", "timer", "toolbar", "tooltip", "tree", "treegrid", "treeitem"
    };

    public override string Id => "aria-valid-role";
    public override string Name => "ARIA role values must be valid WAI-ARIA roles";
    public override RuleCategory Category => RuleCategory.Aria;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.NameRoleValue;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elements = document.QuerySelectorAll("[role]");

        foreach (var el in elements)
        {
            var role = el.GetAttribute("role")?.Trim();
            if (string.IsNullOrWhiteSpace(role)) continue;

            var roles = role.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var r in roles)
            {
                if (!ValidRoles.Contains(r))
                {
                    yield return CreateIssue(
                        el, rawSource,
                        $"Element has an invalid or unrecognized ARIA role '{r}'. Assistive tech will ignore this role.",
                        $"Replace '{r}' with a standardized WAI-ARIA role (e.g. 'button', 'menu', 'dialog', 'navigation').",
                        el.OuterHtml.Replace($"role=\"{role}\"", "role=\"region\"")
                    );
                }
            }
        }
    }
}
