using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Structure;

public class HtmlLangRule : BaseRule
{
    private static readonly Regex ValidLangCode = new(@"^[a-zA-Z]{2,3}(-[a-zA-Z0-9]+)*$", RegexOptions.Compiled);

    public override string Id => "html-has-lang";
    public override string Name => "<html> element must have a valid 'lang' attribute";
    public override RuleCategory Category => RuleCategory.Structure;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.LanguageOfPage;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var html = document.DocumentElement;
        if (html == null) yield break;

        var lang = html.GetAttribute("lang")?.Trim();

        if (string.IsNullOrWhiteSpace(lang))
        {
            yield return CreateIssue(
                html, rawSource,
                "The <html> tag is missing a 'lang' attribute. Screen readers cannot choose the correct pronunciation or voice engine.",
                "Specify the primary document language, e.g., <html lang=\"en\">.",
                "<html lang=\"en\">"
            );
        }
        else if (!ValidLangCode.IsMatch(lang))
        {
            yield return CreateIssue(
                html, rawSource,
                $"The 'lang' attribute value '{lang}' is not a valid BCP 47 language code.",
                "Use a valid BCP 47 language tag such as 'en', 'en-US', 'es', 'fr', 'de'.",
                $"<html lang=\"en\">",
                IssueSeverity.Moderate
            );
        }
    }
}
