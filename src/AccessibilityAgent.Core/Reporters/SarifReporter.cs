using System.Text.Json;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Reporters;

public class SarifReporter : IAuditReporter
{
    public Task<string> GenerateReportAsync(AuditResult result)
    {
        var sarif = new
        {
            version = "2.1.0",
            schema = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json",
            runs = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "AccessibilityAgent",
                            version = "1.0.0",
                            informationUri = "https://github.com/T-Nitisha/AccessibilityAgent",
                            rules = result.Issues.Select(i => new
                            {
                                id = i.RuleId,
                                name = i.RuleName,
                                shortDescription = new { text = i.RuleName },
                                fullDescription = new { text = i.Criterion.Description },
                                helpUri = i.Criterion.HelpUrl,
                                defaultConfiguration = new
                                {
                                    level = MapSeverityToSarif(i.Severity)
                                }
                            }).DistinctBy(r => r.id).ToArray()
                        }
                    },
                    results = result.Issues.Select(i => new
                    {
                        ruleId = i.RuleId,
                        level = MapSeverityToSarif(i.Severity),
                        message = new { text = $"{i.Message}\nRemediation: {i.RemediationSummary}" },
                        locations = new[]
                        {
                            new
                            {
                                physicalLocation = new
                                {
                                    artifactLocation = new { uri = i.TargetFile ?? result.Target },
                                    region = new
                                    {
                                        startLine = Math.Max(1, i.LineNumber),
                                        startColumn = Math.Max(1, i.ColumnNumber),
                                        snippet = new { text = i.HtmlSnippet }
                                    }
                                }
                            }
                        }
                    }).ToArray()
                }
            }
        };

        var json = JsonSerializer.Serialize(sarif, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Task.FromResult(json);
    }

    private static string MapSeverityToSarif(IssueSeverity severity) => severity switch
    {
        IssueSeverity.Critical or IssueSeverity.Serious => "error",
        IssueSeverity.Moderate => "warning",
        _ => "note"
    };
}
