using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class HeadingHierarchyRule : BaseRule
{
    public override string Id => "heading-order";
    public override string Name => "Heading levels must increase by at most one level at a time";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Moderate;
    public override WcagCriterion Criterion => WcagCriterion.InfoAndRelationships;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var headings = document.QuerySelectorAll("h1, h2, h3, h4, h5, h6").ToList();

        if (headings.Count == 0)
        {
            // If it's a full document with body and content, suggest an h1
            if (rawSource.Contains("<body", StringComparison.OrdinalIgnoreCase) && document.Body != null && document.Body.Children.Length > 0)
            {
                yield return CreateIssue(
                    document.Body, rawSource,
                    "Document does not contain any headings (<h1> - <h6>). Assistive tech users rely on headings to skim page structure.",
                    "Add an <h1> heading representing the main subject or title of the page.",
                    "<h1>Page Title</h1>"
                );
            }
            yield break;
        }

        int previousLevel = 0;
        foreach (var heading in headings)
        {
            var levelStr = heading.TagName.Substring(1);
            if (int.TryParse(levelStr, out int currentLevel))
            {
                if (previousLevel == 0)
                {
                    if (currentLevel > 1)
                    {
                        yield return CreateIssue(
                            heading, rawSource,
                            $"First heading on the page is <h{currentLevel}> instead of <h1>.",
                            "Start document structure with an <h1> before introducing subheadings.",
                            $"<h1>{heading.TextContent?.Trim()}</h1>",
                            IssueSeverity.Minor
                        );
                    }
                }
                else if (currentLevel > previousLevel + 1)
                {
                    yield return CreateIssue(
                        heading, rawSource,
                        $"Heading level skipped from <h{previousLevel}> to <h{currentLevel}>. Screen reader users expecting hierarchical order may get confused.",
                        $"Change <h{currentLevel}> to <h{previousLevel + 1}> or insert the intermediate heading level.",
                        $"<h{previousLevel + 1}>{heading.TextContent?.Trim()}</h{previousLevel + 1}>"
                    );
                }

                previousLevel = currentLevel;
            }
        }
    }
}
