using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace AccessibilityAgent.Core.Analysis;

public static class SourceLocationFinder
{
    public static (int Line, int Column) FindLocation(string sourceText, IElement element)
    {
        if (string.IsNullOrEmpty(sourceText) || element == null)
            return (1, 1);

        // Try exact match on opening tag first
        var id = element.Id;
        var tag = element.TagName.ToLowerInvariant();

        string pattern;
        if (!string.IsNullOrEmpty(id))
        {
            pattern = $@"<{tag}[^>]*id=[""']{Regex.Escape(id)}[""']";
        }
        else
        {
            // Build a signature with distinct attributes
            var classAttr = element.ClassName;
            if (!string.IsNullOrEmpty(classAttr))
            {
                pattern = $@"<{tag}[^>]*class=[""'][^""']*{Regex.Escape(classAttr.Split(' ')[0])}[^""']*[""']";
            }
            else
            {
                // Fallback to tag name
                pattern = $@"<{tag}\b";
            }
        }

        try
        {
            var match = Regex.Match(sourceText, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return GetLineAndColumn(sourceText, match.Index);
            }
        }
        catch
        {
            // Ignore regex error and fallback
        }

        return (1, 1);
    }

    public static (int Line, int Column) GetLineAndColumn(string text, int characterIndex)
    {
        if (characterIndex < 0 || characterIndex > text.Length)
            return (1, 1);

        int line = 1;
        int col = 1;
        for (int i = 0; i < characterIndex; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                col = 1;
            }
            else
            {
                col++;
            }
        }

        return (line, col);
    }
}
