namespace AccessibilityAgent.Core.Models;

public record ContrastResult(
    string Foreground,
    string Background,
    double Ratio,
    bool PassesNormalAa,
    bool PassesLargeAa,
    bool PassesNormalAaa,
    bool PassesLargeAaa,
    bool PassesUiComponent)
{
    public string RatingString => Ratio switch
    {
        >= 7.0 => "AAA (Pass)",
        >= 4.5 => "AA (Pass)",
        >= 3.0 => "AA Large / UI Component (Pass)",
        _ => "Fail"
    };
}
