namespace AccessibilityAgent.Core.Models;

public class AuditResult
{
    public required string Target { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public TimeSpan Duration { get; set; }
    public int ElementsEvaluated { get; set; }
    public int RulesEvaluated { get; set; }
    public List<AccessibilityIssue> Issues { get; set; } = new();
    public AccessibilityNode? AccessibilityTree { get; set; }

    public int CriticalCount => Issues.Count(i => i.Severity == IssueSeverity.Critical);
    public int SeriousCount => Issues.Count(i => i.Severity == IssueSeverity.Serious);
    public int ModerateCount => Issues.Count(i => i.Severity == IssueSeverity.Moderate);
    public int MinorCount => Issues.Count(i => i.Severity == IssueSeverity.Minor);
    public int TotalIssues => Issues.Count;

    public int AccessibilityScore
    {
        get
        {
            if (ElementsEvaluated == 0 && Issues.Count == 0) return 100;
            // Weighted penalty model
            int penalty = (CriticalCount * 20) + (SeriousCount * 10) + (ModerateCount * 4) + (MinorCount * 1);
            int score = 100 - penalty;
            return Math.Clamp(score, 0, 100);
        }
    }

    public string Grade => AccessibilityScore switch
    {
        >= 95 => "A+",
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };

    public bool PassesWcagAa => !Issues.Any(i => 
        (i.Criterion.Level == WcagLevel.A || i.Criterion.Level == WcagLevel.AA) && 
        (i.Severity == IssueSeverity.Critical || i.Severity == IssueSeverity.Serious));

    public bool PassesWcagAaa => !Issues.Any(i => 
        i.Severity == IssueSeverity.Critical || i.Severity == IssueSeverity.Serious);
}
