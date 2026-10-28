using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class DocumentTitleRule : BaseRule
{
    public override string Id => "document-title";
    public override string Name => "Documents must have a non-empty <title> element";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.PageTitled;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        // Only enforce on full documents (having <html> or <body>)
        if (document.DocumentElement == null || !rawSource.Contains("<body", StringComparison.OrdinalIgnoreCase))
            yield break;

        var titleElement = document.QuerySelector("head > title, title");
        if (titleElement == null)
        {
            var head = document.Head ?? (IElement)document.DocumentElement;
            yield return CreateIssue(
                head, rawSource,
                "Document has no <title> element in the <head>.",
                "Add a <title> element inside <head> describing the specific page and application context.",
                "<head>\n  <title>Accessible Page Title - Site Name</title>\n</head>"
            );
        }
        else if (string.IsNullOrWhiteSpace(titleElement.TextContent))
        {
            yield return CreateIssue(
                titleElement, rawSource,
                "The <title> element is empty or contains only whitespace.",
                "Provide descriptive text inside <title> explaining what the page does.",
                "<title>Descriptive Page Title</title>"
            );
        }
    }
}
