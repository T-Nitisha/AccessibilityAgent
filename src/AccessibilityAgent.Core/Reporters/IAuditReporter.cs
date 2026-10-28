using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Reporters;

public interface IAuditReporter
{
    Task<string> GenerateReportAsync(AuditResult result);
}
