using System.Globalization;
using System.Text.RegularExpressions;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Analysis;

public static class ContrastCalculator
{
    private static readonly Dictionary<string, (byte R, byte G, byte B)> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        { "black", (0, 0, 0) },
        { "white", (255, 255, 255) },
        { "red", (255, 0, 0) },
        { "green", (0, 128, 0) },
        { "blue", (0, 0, 255) },
        { "yellow", (255, 255, 0) },
        { "cyan", (0, 255, 255) },
        { "magenta", (255, 0, 255) },
        { "gray", (128, 128, 128) },
        { "grey", (128, 128, 128) },
        { "lightgray", (211, 211, 211) },
        { "darkgray", (169, 169, 169) },
        { "orange", (255, 165, 0) },
        { "purple", (128, 0, 128) },
        { "pink", (255, 192, 203) },
        { "silver", (192, 192, 192) },
        { "navy", (0, 0, 128) },
        { "teal", (0, 128, 128) },
        { "lime", (0, 255, 0) },
        { "maroon", (128, 0, 0) },
        { "olive", (128, 128, 0) },
        { "aqua", (0, 255, 255) }
    };

    public static ContrastResult Calculate(string foreground, string background)
    {
        var fg = TryParseColor(foreground) ?? (0, 0, 0); // default to black
        var bg = TryParseColor(background) ?? (255, 255, 255); // default to white

        double l1 = CalculateRelativeLuminance(fg.R, fg.G, fg.B);
        double l2 = CalculateRelativeLuminance(bg.R, bg.G, bg.B);

        double brighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);

        double ratio = (brighter + 0.05) / (darker + 0.05);
        ratio = Math.Round(ratio, 2);

        return new ContrastResult(
            Foreground: foreground,
            Background: background,
            Ratio: ratio,
            PassesNormalAa: ratio >= 4.5,
            PassesLargeAa: ratio >= 3.0,
            PassesNormalAaa: ratio >= 7.0,
            PassesLargeAaa: ratio >= 4.5,
            PassesUiComponent: ratio >= 3.0
        );
    }

    public static double CalculateRelativeLuminance(byte r, byte g, byte b)
    {
        double rs = LinearizeColorComponent(r / 255.0);
        double gs = LinearizeColorComponent(g / 255.0);
        double bs = LinearizeColorComponent(b / 255.0);

        return (0.2126 * rs) + (0.7152 * gs) + (0.0722 * bs);
    }

    private static double LinearizeColorComponent(double c)
    {
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    public static (byte R, byte G, byte B)? TryParseColor(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var colorStr = input.Trim().ToLowerInvariant();

        if (NamedColors.TryGetValue(colorStr, out var named))
        {
            return named;
        }

        // Hex formats: #RGB, #RGBA, #RRGGBB, #RRGGBBAA
        if (colorStr.StartsWith('#'))
        {
            var hex = colorStr.Substring(1);
            if (hex.Length == 3) // #RGB
            {
                byte r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                byte g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                byte b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                return (r, g, b);
            }
            if (hex.Length is 6 or 8) // #RRGGBB or #RRGGBBAA
            {
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return (r, g, b);
            }
        }

        // rgb(r, g, b) or rgba(r, g, b, a)
        var rgbMatch = Regex.Match(colorStr, @"rgba?\s*\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})");
        if (rgbMatch.Success)
        {
            byte r = byte.Parse(rgbMatch.Groups[1].Value);
            byte g = byte.Parse(rgbMatch.Groups[2].Value);
            byte b = byte.Parse(rgbMatch.Groups[3].Value);
            return (r, g, b);
        }

        return null;
    }
}
