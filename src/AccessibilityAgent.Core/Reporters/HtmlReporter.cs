using System.Net;
using System.Text;
using System.Text.Json;
using AccessibilityAgent.Core.Models;

namespace AccessibilityAgent.Core.Reporters;

public class HtmlReporter : IAuditReporter
{
    public Task<string> GenerateReportAsync(AuditResult result)
    {
        var sb = new StringBuilder();

        var scoreColor = result.AccessibilityScore switch
        {
            >= 90 => "#10b981",
            >= 75 => "#3b82f6",
            >= 60 => "#f59e0b",
            _ => "#ef4444"
        };

        sb.Append($@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Accessibility Audit Report - {WebUtility.HtmlEncode(result.Target)}</title>
  <style>
    :root {{
      --bg: #0f172a;
      --card-bg: #1e293b;
      --border: #334155;
      --text: #f8fafc;
      --text-muted: #94a3b8;
      --primary: #6366f1;
      --critical: #ef4444;
      --serious: #f97316;
      --moderate: #eab308;
      --minor: #3b82f6;
      --success: #10b981;
    }}
    * {{ box-sizing: border-box; margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }}
    body {{ background: var(--bg); color: var(--text); padding: 2rem 1rem; line-height: 1.5; }}
    .container {{ max-width: 1200px; margin: 0 auto; }}
    .header {{ display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; flex-wrap: wrap; gap: 1rem; }}
    .title h1 {{ font-size: 1.8rem; display: flex; align-items: center; gap: 0.5rem; }}
    .title p {{ color: var(--text-muted); font-size: 0.9rem; margin-top: 0.25rem; }}
    .score-badge {{ display: flex; align-items: center; gap: 1rem; background: var(--card-bg); padding: 0.75rem 1.5rem; border-radius: 12px; border: 1px solid var(--border); }}
    .score-circle {{ width: 56px; height: 56px; border-radius: 50%; background: {scoreColor}22; border: 3px solid {scoreColor}; display: flex; align-items: center; justify-content: center; font-size: 1.4rem; font-weight: bold; color: {scoreColor}; }}
    
    .stats-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 2rem; }}
    .stat-card {{ background: var(--card-bg); border: 1px solid var(--border); border-radius: 10px; padding: 1.25rem; }}
    .stat-card .label {{ color: var(--text-muted); font-size: 0.85rem; text-transform: uppercase; font-weight: 600; }}
    .stat-card .val {{ font-size: 2rem; font-weight: 700; margin-top: 0.25rem; }}
    .stat-card.critical .val {{ color: var(--critical); }}
    .stat-card.serious .val {{ color: var(--serious); }}
    .stat-card.moderate .val {{ color: var(--moderate); }}
    .stat-card.minor .val {{ color: var(--minor); }}

    .toolbar {{ display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 1rem; }}
    .filters {{ display: flex; gap: 0.5rem; flex-wrap: wrap; }}
    .filter-btn {{ background: var(--card-bg); border: 1px solid var(--border); color: var(--text); padding: 0.4rem 0.9rem; border-radius: 6px; cursor: pointer; font-size: 0.85rem; transition: all 0.2s; }}
    .filter-btn.active, .filter-btn:hover {{ background: var(--primary); border-color: var(--primary); }}

    .section-title {{ font-size: 1.3rem; margin-bottom: 1rem; display: flex; align-items: center; gap: 0.5rem; }}

    .issue-card {{ background: var(--card-bg); border: 1px solid var(--border); border-radius: 10px; padding: 1.5rem; margin-bottom: 1rem; transition: border-color 0.2s; }}
    .issue-card:hover {{ border-color: var(--primary); }}
    .issue-header {{ display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; margin-bottom: 0.75rem; flex-wrap: wrap; }}
    .issue-title {{ font-size: 1.1rem; font-weight: 600; display: flex; align-items: center; gap: 0.6rem; }}
    .badge {{ font-size: 0.75rem; padding: 0.2rem 0.6rem; border-radius: 9999px; font-weight: 600; text-transform: uppercase; }}
    .badge.critical {{ background: var(--critical)22; color: var(--critical); border: 1px solid var(--critical); }}
    .badge.serious {{ background: var(--serious)22; color: var(--serious); border: 1px solid var(--serious); }}
    .badge.moderate {{ background: var(--moderate)22; color: var(--moderate); border: 1px solid var(--moderate); }}
    .badge.minor {{ background: var(--minor)22; color: var(--minor); border: 1px solid var(--minor); }}
    .wcag-tag {{ background: #4338ca33; color: #a5b4fc; border: 1px solid #6366f1; padding: 0.2rem 0.6rem; border-radius: 6px; font-size: 0.8rem; text-decoration: none; }}
    .wcag-tag:hover {{ text-decoration: underline; }}
    
    .location-info {{ color: var(--text-muted); font-size: 0.85rem; margin-bottom: 0.75rem; font-family: monospace; }}
    .issue-msg {{ margin-bottom: 1rem; color: #cbd5e1; }}
    .remediation-box {{ background: #0f172a; border-left: 4px solid var(--success); padding: 0.75rem 1rem; border-radius: 4px; margin-bottom: 1rem; font-size: 0.9rem; }}
    .remediation-box strong {{ color: var(--success); display: block; margin-bottom: 0.25rem; }}

    .code-tabs {{ margin-top: 1rem; }}
    .code-snippet {{ background: #090d16; border: 1px solid var(--border); border-radius: 6px; padding: 0.75rem; font-family: 'Consolas', 'Courier New', monospace; font-size: 0.85rem; overflow-x: auto; color: #e2e8f0; }}
    .code-label {{ font-size: 0.75rem; color: var(--text-muted); text-transform: uppercase; font-weight: 600; margin-bottom: 0.25rem; display: block; }}
    .fix-snippet {{ border-color: #10b98166; background: #064e3b18; color: #6ee7b7; }}

    .tree-view {{ background: var(--card-bg); border: 1px solid var(--border); border-radius: 10px; padding: 1.5rem; margin-top: 2rem; font-family: monospace; font-size: 0.9rem; }}
    .tree-node {{ margin-left: 1.5rem; padding: 0.2rem 0; }}
    .role {{ color: #818cf8; font-weight: bold; }}
    .name {{ color: #34d399; }}
    .state {{ color: #f59e0b; font-size: 0.8rem; }}
  </style>
</head>
<body>
  <div class=""container"">
    <header class=""header"">
      <div class=""title"">
        <h1>♿ Accessibility Audit Report</h1>
        <p>Target: <strong>{WebUtility.HtmlEncode(result.Target)}</strong> &bull; Scanned: {result.Timestamp:yyyy-MM-dd HH:mm:ss UTC}</p>
      </div>
      <div class=""score-badge"">
        <div class=""score-circle"">{result.AccessibilityScore}</div>
        <div>
          <div style=""font-weight: 600; font-size: 1.1rem;"">Grade: {result.Grade}</div>
          <div style=""font-size: 0.8rem; color: var(--text-muted);"">{result.TotalIssues} issues detected</div>
        </div>
      </div>
    </header>

    <div class=""stats-grid"">
      <div class=""stat-card critical""><div class=""label"">Critical</div><div class=""val"">{result.CriticalCount}</div></div>
      <div class=""stat-card serious""><div class=""label"">Serious</div><div class=""val"">{result.SeriousCount}</div></div>
      <div class=""stat-card moderate""><div class=""label"">Moderate</div><div class=""val"">{result.ModerateCount}</div></div>
      <div class=""stat-card minor""><div class=""label"">Minor</div><div class=""val"">{result.MinorCount}</div></div>
    </div>

    <div class=""toolbar"">
      <div class=""section-title"">Audited Findings ({result.Issues.Count})</div>
      <div class=""filters"">
        <button class=""filter-btn active"" onclick=""filterIssues('all')"">All ({result.Issues.Count})</button>
        <button class=""filter-btn"" onclick=""filterIssues('Critical')"">Critical ({result.CriticalCount})</button>
        <button class=""filter-btn"" onclick=""filterIssues('Serious')"">Serious ({result.SeriousCount})</button>
        <button class=""filter-btn"" onclick=""filterIssues('Moderate')"">Moderate ({result.ModerateCount})</button>
        <button class=""filter-btn"" onclick=""filterIssues('Minor')"">Minor ({result.MinorCount})</button>
      </div>
    </div>

    <div id=""issues-container"">");

        foreach (var issue in result.Issues)
        {
            var sevClass = issue.Severity.ToString().ToLowerInvariant();
            sb.Append($@"
      <div class=""issue-card"" data-severity=""{issue.Severity}"">
        <div class=""issue-header"">
          <div class=""issue-title"">
            <span class=""badge {sevClass}"">{issue.Severity}</span>
            <span>[{issue.RuleId}] {WebUtility.HtmlEncode(issue.RuleName)}</span>
          </div>
          <a class=""wcag-tag"" href=""{issue.Criterion.HelpUrl}"" target=""_blank"" rel=""noopener noreferrer"">
            WCAG {issue.Criterion.Number} ({issue.Criterion.Level})
          </a>
        </div>
        <div class=""location-info"">📍 {WebUtility.HtmlEncode(issue.FormattedLocation)} &bull; <code>{WebUtility.HtmlEncode(issue.Selector)}</code></div>
        <div class=""issue-msg"">{WebUtility.HtmlEncode(issue.Message)}</div>
        <div class=""remediation-box"">
          <strong>💡 Recommended Fix:</strong>
          {WebUtility.HtmlEncode(issue.RemediationSummary)}
        </div>");

            if (!string.IsNullOrEmpty(issue.HtmlSnippet))
            {
                sb.Append($@"
        <div class=""code-tabs"">
          <span class=""code-label"">Current Code</span>
          <pre class=""code-snippet""><code>{WebUtility.HtmlEncode(issue.HtmlSnippet)}</code></pre>
        </div>");
            }

            if (!string.IsNullOrEmpty(issue.SuggestedFixCode))
            {
                sb.Append($@"
        <div class=""code-tabs"" style=""margin-top: 0.5rem;"">
          <span class=""code-label"" style=""color: #34d399;"">Suggested Fix</span>
          <pre class=""code-snippet fix-snippet""><code>{WebUtility.HtmlEncode(issue.SuggestedFixCode)}</code></pre>
        </div>");
            }

            sb.Append("      </div>");
        }

        sb.Append(@"
    </div>");

        // Accessibility Tree Section if available
        if (result.AccessibilityTree != null)
        {
            sb.Append(@"
    <div class=""section-title"" style=""margin-top: 2.5rem;"">🌳 Accessibility Object Model Tree (Assistive Technology View)</div>
    <div class=""tree-view"">");
            RenderTreeHtml(sb, result.AccessibilityTree, 0);
            sb.Append(@"
    </div>");
        }

        sb.Append(@"
  </div>
  <script>
    function filterIssues(severity) {
      document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
      event.target.classList.add('active');
      const cards = document.querySelectorAll('.issue-card');
      cards.forEach(card => {
        if (severity === 'all' || card.getAttribute('data-severity') === severity) {
          card.style.display = 'block';
        } else {
          card.style.display = 'none';
        }
      });
    }
  </script>
</body>
</html>");

        return Task.FromResult(sb.ToString());
    }

    private static void RenderTreeHtml(StringBuilder sb, AccessibilityNode node, int depth)
    {
        var indent = depth * 1.5;
        sb.Append($"<div class=\"tree-node\" style=\"margin-left: {indent}rem;\">");
        sb.Append($"<span class=\"role\">{WebUtility.HtmlEncode(node.Role)}</span>");

        if (!string.IsNullOrEmpty(node.Name))
        {
            sb.Append($" <span class=\"name\">\"{WebUtility.HtmlEncode(node.Name)}\"</span>");
        }

        if (node.States.Any())
        {
            var statesStr = string.Join(", ", node.States.Select(s => $"{s.Key}={s.Value}"));
            sb.Append($" <span class=\"state\">[{WebUtility.HtmlEncode(statesStr)}]</span>");
        }

        sb.Append("</div>");

        foreach (var child in node.Children.Take(50)) // limit depth/count for performance
        {
            RenderTreeHtml(sb, child, depth + 1);
        }
    }
}
