using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class MainLandmarkRule : BaseRule
{
    public override string Id => "main-landmark";
    public override string Name => "Documents must have one distinct <main> landmark";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Moderate;
    public override WcagCriterion Criterion => WcagCriterion.BypassBlocks;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        if (!rawSource.Contains("<body", StringComparison.OrdinalIgnoreCase))
            yield break;

        var mains = document.QuerySelectorAll("main, [role='main']").ToList();

        if (mains.Count == 0)
        {
            if (document.Body != null)
            {
                yield return CreateIssue(
                    document.Body, rawSource,
                    "No <main> or role='main' landmark found. Screen reader users cannot quickly jump to the primary content.",
                    "Wrap the primary body content inside a <main> tag.",
                    "<main>\n  <!-- Primary page content -->\n</main>"
                );
            }
        }
        else if (mains.Count > 1)
        {
            foreach (var main in mains.Skip(1))
            {
                yield return CreateIssue(
                    main, rawSource,
                    "Multiple <main> landmarks detected. A document must only have one visible main landmark.",
                    "Ensure only one <main> element is present or designate secondary sections with appropriate roles (such as <aside> or <section aria-label='...'>).",
                    null,
                    IssueSeverity.Minor
                );
            }
        }
    }
}
