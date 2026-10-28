using AngleSharp.Dom;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Rules.Media;

public class MediaCaptionsRule : BaseRule
{
    public override string Id => "media-captions";
    public override string Name => "<video> elements must provide closed captions";
    public override RuleCategory Category => RuleCategory.Media;
    public override IssueSeverity Severity => IssueSeverity.Critical;
    public override WcagCriterion Criterion => WcagCriterion.CaptionsPrerecorded;

    public override IEnumerable<AccessibilityIssue> Evaluate(IDocument document, string rawSource)
    {
        var videos = document.QuerySelectorAll("video");

        foreach (var video in videos)
        {
            var captionTrack = video.QuerySelector("track[kind='captions'], track[kind='subtitles']");
            if (captionTrack == null)
            {
                yield return CreateIssue(
                    video, rawSource,
                    "<video> element does not contain a <track kind='captions'>. Deaf and hard-of-hearing users cannot consume audio information.",
                    "Add a <track kind='captions' src='captions.vtt' srclang='en' label='English'> inside the <video> tag.",
                    video.OuterHtml.Replace("</video>", "  <track kind=\"captions\" src=\"captions.vtt\" srclang=\"en\" label=\"English\">\n</video>")
                );
            }
        }
    }
}
