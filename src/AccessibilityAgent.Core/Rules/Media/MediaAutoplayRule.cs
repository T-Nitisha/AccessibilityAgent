using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Media;

public class MediaAutoplayRule : BaseRule
{
    public override string Id => "media-autoplay";
    public override string Name => "Audio and video must not autoplay without controls or mute";
    public override RuleCategory Category => RuleCategory.Media;
    public override IssueSeverity Severity => IssueSeverity.Serious;
    public override WcagCriterion Criterion => WcagCriterion.NonTextContent;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var mediaElements = document.QuerySelectorAll("video[autoplay], audio[autoplay]");

        foreach (var media in mediaElements)
        {
            var isMuted = media.HasAttribute("muted");
            var hasControls = media.HasAttribute("controls");

            if (!isMuted && !hasControls)
            {
                yield return CreateIssue(
                    media, rawSource,
                    $"<{media.TagName.ToLowerInvariant()}> autoplays without mute or playback controls. Sudden audio disrupts screen readers and causes disorientation.",
                    "Add the 'muted' attribute to autoplaying media, or include 'controls' and require explicit user action to start playback.",
                    media.OuterHtml.Replace("autoplay", "autoplay muted controls")
                );
            }
        }
    }
}
