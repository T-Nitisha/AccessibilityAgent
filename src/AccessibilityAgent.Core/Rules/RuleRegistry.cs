using AccessibilityAgent.Core.Models;
using AccessibilityAgent.Core.Rules.Aria;
using AccessibilityAgent.Core.Rules.Contrast;
using AccessibilityAgent.Core.Rules.Forms;
using AccessibilityAgent.Core.Rules.Images;
using AccessibilityAgent.Core.Rules.Keyboard;
using AccessibilityAgent.Core.Rules.Media;
using AccessibilityAgent.Core.Rules.Navigation;
using AccessibilityAgent.Core.Rules.Structure;

namespace AccessibilityAgent.Core.Rules;

public class RuleRegistry
{
    private readonly List<IAccessibilityRule> _rules = new();

    public RuleRegistry()
    {
        RegisterDefaultRules();
    }

    public IReadOnlyList<IAccessibilityRule> Rules => _rules;

    public void Register(IAccessibilityRule rule)
    {
        if (!_rules.Any(r => r.Id.Equals(rule.Id, StringComparison.OrdinalIgnoreCase)))
        {
            _rules.Add(rule);
        }
    }

    public IEnumerable<IAccessibilityRule> GetRules(
        WcagLevel maxLevel = WcagLevel.AAA,
        IssueSeverity minSeverity = IssueSeverity.Info,
        RuleCategory? category = null)
    {
        return _rules.Where(r =>
            r.Criterion.Level <= maxLevel &&
            r.Severity >= minSeverity &&
            (!category.HasValue || r.Category == category.Value));
    }

    private void RegisterDefaultRules()
    {
        // Images
        Register(new ImgAltRule());
        Register(new SvgAccessibleNameRule());

        // Structure
        Register(new HtmlLangRule());
        Register(new DocumentTitleRule());
        Register(new HeadingHierarchyRule());
        Register(new MainLandmarkRule());
        Register(new TableHeadersRule());
        Register(new ListStructureRule());

        // Forms
        Register(new InputLabelRule());
        Register(new FieldsetLegendRule());
        Register(new FormAutocompleteRule());

        // Navigation
        Register(new ButtonNameRule());
        Register(new LinkTextRule());
        Register(new LinkNewTabRule());

        // Keyboard
        Register(new KeyboardNavigableRule());
        Register(new TabIndexPositiveRule());
        Register(new FocusOutlineRule());
        Register(new SkipLinkRule());

        // ARIA
        Register(new ValidAriaRoleRule());
        Register(new ValidAriaAttributeRule());
        Register(new AriaHiddenFocusableRule());
        Register(new DuplicateIdRule());

        // Contrast
        Register(new ColorContrastRule());

        // Media
        Register(new MediaCaptionsRule());
        Register(new MediaAutoplayRule());
    }
}
