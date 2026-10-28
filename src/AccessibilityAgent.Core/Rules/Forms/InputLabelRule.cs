using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Forms;

public class InputLabelRule : BaseRule
{
    private static readonly HashSet<string> IgnoredTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "hidden", "submit", "reset", "button", "image"
    };

    public override string Id => "form-input-label";
    public override string Name => "Form controls must have an associated accessible label";
    public override RuleCategory Category => RuleCategory.Forms;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.NameRoleValue;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var controls = document.QuerySelectorAll("input, select, textarea");

        foreach (var control in controls)
        {
            var tag = control.TagName.ToLowerInvariant();
            if (tag == "input")
            {
                var type = control.GetAttribute("type") ?? "text";
                if (IgnoredTypes.Contains(type))
                    continue;
            }

            // 1. Check aria-label / aria-labelledby
            if (!string.IsNullOrWhiteSpace(control.GetAttribute("aria-label")) ||
                !string.IsNullOrWhiteSpace(control.GetAttribute("aria-labelledby")))
            {
                continue;
            }

            // 2. Check title attribute
            if (!string.IsNullOrWhiteSpace(control.GetAttribute("title")))
            {
                continue;
            }

            // 3. Check for matching label[for="id"]
            var id = control.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var matchingLabel = document.QuerySelector($"label[for='{id}']");
                if (matchingLabel != null && !string.IsNullOrWhiteSpace(matchingLabel.TextContent))
                {
                    continue;
                }
            }

            // 4. Check enclosing <label>
            var enclosingLabel = control.Closest("label");
            if (enclosingLabel != null && !string.IsNullOrWhiteSpace(enclosingLabel.TextContent))
            {
                continue;
            }

            // If we reached here, no accessible label exists
            string suggestedId = string.IsNullOrWhiteSpace(id) ? "input-field" : id;
            string suggestedFix = string.IsNullOrWhiteSpace(id)
                ? $"<label for=\"{suggestedId}\">Field Label</label>\n{control.OuterHtml.Replace("<input", $"<input id=\"{suggestedId}\"")}"
                : $"<label for=\"{suggestedId}\">Field Label</label>\n{control.OuterHtml}";

            yield return CreateIssue(
                control, rawSource,
                $"Form element <{tag}> has no accessible label. Screen reader users will not know what data is requested.",
                "Associate a <label for='...'> matching the element's id, wrap it in a <label>, or provide an aria-label attribute.",
                suggestedFix
            );
        }
    }
}
