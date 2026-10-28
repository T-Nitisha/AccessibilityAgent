using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Aria;

public class DuplicateIdRule : BaseRule
{
    public override string Id => "duplicate-id";
    public override string Name => "All id attribute values on a page must be unique";
    public override RuleCategory Category => RuleCategory.Aria;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.InfoAndRelationships;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var elementsWithId = document.QuerySelectorAll("[id]")
            .GroupBy(e => e.Id)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1);

        foreach (var group in elementsWithId)
        {
            var duplicates = group.Skip(1);
            foreach (var dup in duplicates)
            {
                yield return CreateIssue(
                    dup, rawSource,
                    $"Duplicate id='{group.Key}' detected. Multiple elements share this id, breaking labels, ARIA references, and anchor links.",
                    $"Change the id on this element to be unique across the entire DOM tree.",
                    dup.OuterHtml.Replace($"id=\"{group.Key}\"", $"id=\"{group.Key}-2\"")
                );
            }
        }
    }
}
