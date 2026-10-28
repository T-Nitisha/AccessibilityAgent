using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class ListStructureRule : BaseRule
{
    public override string Id => "list-structure";
    public override string Name => "<ul> and <ol> lists must contain only <li> elements";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Moderate;
    public override WcagCriterion Criterion => WcagCriterion.InfoAndRelationships;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var lists = document.QuerySelectorAll("ul, ol");

        foreach (var list in lists)
        {
            foreach (var child in list.Children)
            {
                var tag = child.TagName.ToLowerInvariant();
                if (tag is not "li" and not "script" and not "template")
                {
                    yield return CreateIssue(
                        child, rawSource,
                        $"List contains <{tag}> as a direct child. Direct children of <{list.TagName.ToLowerInvariant()}> must only be <li>.",
                        $"Wrap the <{tag}> element inside an <li> element or relocate it outside the list.",
                        $"<li>{child.OuterHtml}</li>"
                    );
                }
            }
        }
    }
}
