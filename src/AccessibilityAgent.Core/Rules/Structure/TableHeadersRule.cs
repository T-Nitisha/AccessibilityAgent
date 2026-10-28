using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class TableHeadersRule : BaseRule
{
    public override string Id => "table-headers";
    public override string Name => "Data tables must have <th> header cells with scope";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.InfoAndRelationships;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var tables = document.QuerySelectorAll("table");

        foreach (var table in tables)
        {
            var role = table.GetAttribute("role")?.ToLowerInvariant();
            if (role is "presentation" or "none")
                continue; // Layout table

            var ths = table.QuerySelectorAll("th").ToList();
            if (ths.Count == 0)
            {
                yield return CreateIssue(
                    table, rawSource,
                    "Table does not contain any <th> header cells. Screen reader users cannot understand column/row associations.",
                    "Add <th> elements in the <thead> or first row/column with scope='col' or scope='row'.",
                    "<thead>\n  <tr>\n    <th scope=\"col\">Header</th>\n  </tr>\n</thead>"
                );
            }
            else
            {
                // Check if any <th> lacks scope in multi-column tables
                foreach (var th in ths)
                {
                    if (!th.HasAttribute("scope"))
                    {
                        yield return CreateIssue(
                            th, rawSource,
                            "<th> header cell is missing a 'scope' attribute (scope='col' or scope='row').",
                            "Add scope='col' for column headers or scope='row' for row headers to make relationships explicit.",
                            th.OuterHtml.Replace("<th", "<th scope=\"col\""),
                            IssueSeverity.Minor
                        );
                    }
                }
            }
        }
    }
}
