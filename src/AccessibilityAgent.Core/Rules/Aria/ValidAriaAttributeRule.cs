using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Aria;

public class ValidAriaAttributeRule : BaseRule
{
    private static readonly HashSet<string> ValidAriaAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "aria-activedescendant", "aria-atomic", "aria-autocomplete", "aria-braillelabel",
        "aria-brailleroledescription", "aria-busy", "aria-checked", "aria-colcount",
        "aria-colindex", "aria-colindextext", "aria-colspan", "aria-controls", "aria-current",
        "aria-describedby", "aria-description", "aria-details", "aria-disabled", "aria-dropeffect",
        "aria-errormessage", "aria-expanded", "aria-flowto", "aria-grabbed", "aria-haspopup",
        "aria-hidden", "aria-invalid", "aria-keyshortcuts", "aria-label", "aria-labelledby",
        "aria-level", "aria-live", "aria-modal", "aria-multiline", "aria-multiselectable",
        "aria-orientation", "aria-owns", "aria-placeholder", "aria-posinset", "aria-pressed",
        "aria-readonly", "aria-relevant", "aria-required", "aria-roledescription", "aria-rowcount",
        "aria-rowindex", "aria-rowindextext", "aria-rowspan", "aria-selected", "aria-setsize",
        "aria-sort", "aria-valuemax", "aria-valuemin", "aria-valuenow", "aria-valuetext"
    };

    public override string Id => "aria-valid-attribute";
    public override string Name => "All aria-* attributes must be valid WAI-ARIA attributes";
    public override RuleCategory Category => RuleCategory.Aria;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.NameRoleValue;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elements = document.All;

        foreach (var el in elements)
        {
            foreach (var attr in el.Attributes)
            {
                if (attr.Name.StartsWith("aria-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ValidAriaAttributes.Contains(attr.Name))
                    {
                        string suggestion = attr.Name.ToLowerInvariant() switch
                        {
                            "aria-labelled-by" => "aria-labelledby",
                            "aria-described-by" => "aria-describedby",
                            "aria-role" => "role",
                            _ => "a valid ARIA attribute"
                        };

                        yield return CreateIssue(
                            el, rawSource,
                            $"Unknown or misspelled ARIA attribute '{attr.Name}'.",
                            $"Correct the attribute name to '{suggestion}'. Check W3C WAI-ARIA specification.",
                            null
                        );
                    }
                }
            }
        }
    }
}
