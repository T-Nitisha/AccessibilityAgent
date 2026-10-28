using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Forms;

public class FieldsetLegendRule : BaseRule
{
    public override string Id => "form-fieldset-legend";
    public override string Name => "Radio and checkbox groups must be wrapped in a <fieldset> with <legend>";
    public override RuleCategory Category => RuleCategory.Forms;
    public override IssueSeverity Severity => IssueSeverity.Moderate;
    public override WcagCriterion Criterion => WcagCriterion.InfoAndRelationships;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        // Group radio buttons by name
        var radios = document.QuerySelectorAll("input[type='radio'][name]")
            .GroupBy(r => r.GetAttribute("name")!)
            .Where(g => g.Count() > 1);

        foreach (var group in radios)
        {
            var first = group.First();
            var fieldset = first.Closest("fieldset");
            var legend = fieldset?.QuerySelector("legend");

            if (fieldset == null || legend == null || string.IsNullOrWhiteSpace(legend.TextContent))
            {
                yield return CreateIssue(
                    first, rawSource,
                    $"Radio group with name='{group.Key}' is not enclosed in a <fieldset> with a descriptive <legend>.",
                    "Wrap the related radio buttons inside <fieldset><legend>Group Title</legend>...</fieldset> so assistive tech announces the group context.",
                    $"<fieldset>\n  <legend>Select an option</legend>\n  <!-- radio buttons -->\n</fieldset>"
                );
            }
        }
    }
}
