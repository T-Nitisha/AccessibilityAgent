using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Forms;

public class FormAutocompleteRule : BaseRule
{
    private static readonly HashSet<string> SensitiveFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "username", "password", "tel", "phone", "name", "fname", "lname", 
        "first-name", "last-name", "zip", "postal-code", "address", "country", "credit-card"
    };

    public override string Id => "form-autocomplete";
    public override string Name => "Input fields for user identity should have valid autocomplete attributes";
    public override RuleCategory Category => RuleCategory.Forms;
    public override IssueSeverity Severity => IssueSeverity.Minor;
    public override WcagCriterion Criterion => WcagCriterion.IdentifyInputPurpose;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var inputs = document.QuerySelectorAll("input:not([type='hidden']):not([type='submit']):not([type='button'])");

        foreach (var input in inputs)
        {
            var name = input.GetAttribute("name") ?? input.Id ?? "";
            if (string.IsNullOrWhiteSpace(name)) continue;

            if (SensitiveFieldNames.Any(s => name.Contains(s, StringComparison.OrdinalIgnoreCase)))
            {
                if (!input.HasAttribute("autocomplete"))
                {
                    string suggested = name.ToLowerInvariant() switch
                    {
                        var n when n.Contains("email") => "email",
                        var n when n.Contains("pass") => "current-password",
                        var n when n.Contains("user") => "username",
                        var n when n.Contains("tel") || n.Contains("phone") => "tel",
                        _ => "on"
                    };

                    yield return CreateIssue(
                        input, rawSource,
                        $"Input '{name}' collects personal information but lacks an 'autocomplete' attribute (WCAG 2.1 AA 1.3.5).",
                        $"Add autocomplete='{suggested}' to help users with cognitive or motor impairments fill forms automatically.",
                        input.OuterHtml.Replace("<input", $"<input autocomplete=\"{suggested}\"")
                    );
                }
            }
        }
    }
}
