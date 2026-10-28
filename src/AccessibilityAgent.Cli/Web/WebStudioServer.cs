using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AccessibilityAgent.Core.Analysis;
using AccessibilityAgent.Core.Engine;
using AccessibilityAgent.Core.Models;
using AccessibilityAgent.Core.Reporters;

namespace AccessibilityAgent.Cli.Web;

public static class WebStudioServer
{
    public static async Task StartAsync(int port = 5050, CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://localhost:{port}");
        builder.Services.AddRouting();

        var app = builder.Build();
        var engine = new AccessibilityEngine();

        app.MapGet("/", async (HttpContext context) =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(GetStudioHtml(port));
        });

        app.MapPost("/api/audit", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var req = JsonSerializer.Deserialize<AuditRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (req == null || (string.IsNullOrWhiteSpace(req.Html) && string.IsNullOrWhiteSpace(req.Url)))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "Either html or url must be provided." });
                return;
            }

            var level = req.Level?.ToUpperInvariant() switch
            {
                "A" => WcagLevel.A,
                "AAA" => WcagLevel.AAA,
                _ => WcagLevel.AA
            };

            var options = new AccessibilityEngineOptions { Level = level };
            AuditResult result;

            if (!string.IsNullOrWhiteSpace(req.Url))
            {
                result = await engine.AuditUrlAsync(req.Url, options);
            }
            else
            {
                result = await engine.AuditHtmlAsync(req.Html!, "StudioSnippet", options);
            }

            var htmlReporter = new HtmlReporter();
            var reportHtml = await htmlReporter.GenerateReportAsync(result);

            await context.Response.WriteAsJsonAsync(new
            {
                result.AccessibilityScore,
                result.Grade,
                result.PassesWcagAa,
                result.PassesWcagAaa,
                result.CriticalCount,
                result.SeriousCount,
                result.ModerateCount,
                result.MinorCount,
                result.TotalIssues,
                Issues = result.Issues,
                ReportHtml = reportHtml,
                Tree = result.AccessibilityTree
            });
        });

        app.MapPost("/api/contrast", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var req = JsonSerializer.Deserialize<ContrastRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (req == null || string.IsNullOrWhiteSpace(req.Foreground) || string.IsNullOrWhiteSpace(req.Background))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { error = "Foreground and background colors required." });
                return;
            }

            var result = ContrastCalculator.Calculate(req.Foreground, req.Background);
            await context.Response.WriteAsJsonAsync(result);
        });

        await app.RunAsync(cancellationToken);
    }

    private static string GetStudioHtml(int port) => @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Accessibility Agent - Reviewer & Developer Studio</title>
  <style>
    :root {
      --bg: #0b0f19;
      --card-bg: #151d30;
      --card-alt: #1c2742;
      --border: #293859;
      --text: #f1f5f9;
      --text-muted: #94a3b8;
      --primary: #6366f1;
      --primary-hover: #4f46e5;
      --critical: #ef4444;
      --serious: #f97316;
      --moderate: #eab308;
      --minor: #3b82f6;
      --success: #10b981;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }
    body { background: var(--bg); color: var(--text); padding: 1.5rem; line-height: 1.5; }
    .header { max-width: 1300px; margin: 0 auto 1.5rem auto; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--border); padding-bottom: 1rem; }
    .header h1 { font-size: 1.6rem; display: flex; align-items: center; gap: 0.5rem; }
    .header p { color: var(--text-muted); font-size: 0.9rem; }
    .container { max-width: 1300px; margin: 0 auto; display: grid; grid-template-columns: 1fr 1.2fr; gap: 1.5rem; }
    @media (max-width: 900px) { .container { grid-template-columns: 1fr; } }
    .panel { background: var(--card-bg); border: 1px solid var(--border); border-radius: 12px; padding: 1.5rem; }
    .tabs { display: flex; gap: 0.5rem; margin-bottom: 1rem; border-bottom: 1px solid var(--border); padding-bottom: 0.5rem; }
    .tab-btn { background: transparent; border: none; color: var(--text-muted); padding: 0.5rem 1rem; border-radius: 6px; cursor: pointer; font-weight: 600; font-size: 0.9rem; }
    .tab-btn.active { background: var(--primary); color: white; }
    .form-group { margin-bottom: 1.2rem; }
    label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 0.4rem; color: #cbd5e1; }
    textarea, input, select { width: 100%; background: #0c1220; border: 1px solid var(--border); border-radius: 6px; padding: 0.6rem 0.8rem; color: var(--text); font-size: 0.9rem; }
    textarea { font-family: 'Consolas', 'Courier New', monospace; resize: vertical; min-height: 220px; }
    button.btn-primary { background: var(--primary); color: white; border: none; padding: 0.7rem 1.5rem; border-radius: 6px; font-weight: 600; cursor: pointer; width: 100%; font-size: 1rem; transition: background 0.2s; }
    button.btn-primary:hover { background: var(--primary-hover); }
    .results-empty { text-align: center; padding: 3rem 1rem; color: var(--text-muted); }
    .score-banner { display: flex; align-items: center; justify-content: space-between; background: var(--card-alt); border-radius: 8px; padding: 1rem; margin-bottom: 1rem; border: 1px solid var(--border); }
    .score-circle { width: 50px; height: 50px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 1.3rem; }
    .issue-item { background: #0d1527; border: 1px solid var(--border); border-radius: 8px; padding: 1rem; margin-bottom: 0.75rem; }
    .issue-item.critical { border-left: 4px solid var(--critical); }
    .issue-item.serious { border-left: 4px solid var(--serious); }
    .issue-item.moderate { border-left: 4px solid var(--moderate); }
    .issue-item.minor { border-left: 4px solid var(--minor); }
    .badge { font-size: 0.7rem; font-weight: 700; padding: 0.15rem 0.4rem; border-radius: 4px; text-transform: uppercase; }
    .badge.critical { background: var(--critical)33; color: var(--critical); }
    .badge.serious { background: var(--serious)33; color: var(--serious); }
    .badge.moderate { background: var(--moderate)33; color: var(--moderate); }
    .badge.minor { background: var(--minor)33; color: var(--minor); }
    .fix-box { background: #064e3b22; border: 1px solid #10b98155; border-radius: 6px; padding: 0.5rem 0.75rem; margin-top: 0.5rem; font-family: monospace; font-size: 0.8rem; color: #6ee7b7; white-space: pre-wrap; }
    .contrast-box { display: flex; gap: 1rem; align-items: center; margin-top: 1rem; padding: 1rem; border-radius: 8px; border: 1px solid var(--border); }
    .contrast-preview { padding: 1rem 2rem; border-radius: 6px; font-weight: bold; font-size: 1.1rem; text-align: center; }
  </style>
</head>
<body>
  <div class=""header"">
    <div>
      <h1>♿ Accessibility Agent Studio</h1>
      <p>Interactive WCAG 2.1 / 2.2 Auditor, Code Reviewer & Assistive Tech Inspector for Developers</p>
    </div>
    <div style=""color: var(--text-muted); font-size: 0.85rem;"">Running on http://localhost:" + port + @"</div>
  </div>

  <div class=""container"">
    <div class=""panel"">
      <div class=""tabs"">
        <button class=""tab-btn active"" onclick=""switchTab('code')"">HTML / Component Code</button>
        <button class=""tab-btn"" onclick=""switchTab('url')"">Live URL</button>
        <button class=""tab-btn"" onclick=""switchTab('contrast')"">Contrast Tool</button>
      </div>

      <div id=""tab-code"">
        <div class=""form-group"">
          <label>HTML / Razor Markup to Audit:</label>
          <textarea id=""html-input"" placeholder=""Paste HTML, Razor, or JSX snippet here..."">
<div class=""card"" onclick=""goToProfile()"">
  <img src=""avatar.png"">
  <h2>John Doe</h2>
  <p style=""color: #999; background-color: #fff;"">Software Engineer</p>
  <button><i class=""icon-edit""></i></button>
  <a href=""https://example.com"" target=""_blank"">Learn more</a>
</div></textarea>
        </div>
        <div class=""form-group"">
          <label>WCAG Standard Target:</label>
          <select id=""wcag-level"">
            <option value=""AA"" selected>WCAG 2.1 / 2.2 AA (Standard Recommended)</option>
            <option value=""AAA"">WCAG 2.1 / 2.2 AAA (Enhanced)</option>
            <option value=""A"">WCAG Level A (Minimum)</option>
          </select>
        </div>
        <button class=""btn-primary"" onclick=""runAudit('code')"">Audit Accessibility</button>
      </div>

      <div id=""tab-url"" style=""display: none;"">
        <div class=""form-group"">
          <label>Target Web URL:</label>
          <input type=""url"" id=""url-input"" placeholder=""https://example.com or http://localhost:3000"" value=""https://example.com"">
        </div>
        <button class=""btn-primary"" onclick=""runAudit('url')"">Fetch & Audit URL</button>
      </div>

      <div id=""tab-contrast"" style=""display: none;"">
        <div class=""form-group"">
          <label>Foreground (Text) Color:</label>
          <input type=""text"" id=""fg-input"" value=""#4f46e5"">
        </div>
        <div class=""form-group"">
          <label>Background Color:</label>
          <input type=""text"" id=""bg-input"" value=""#ffffff"">
        </div>
        <button class=""btn-primary"" onclick=""checkContrast()"">Test Contrast</button>
        <div id=""contrast-output"" style=""margin-top: 1rem;""></div>
      </div>
    </div>

    <div class=""panel"">
      <h2 style=""font-size: 1.2rem; margin-bottom: 1rem;"">Audit Results & Reviewer Findings</h2>
      <div id=""results-view"">
        <div class=""results-empty"">
          <p>👈 Select an input method on the left and click <strong>Audit Accessibility</strong> to inspect WCAG violations and remediation code.</p>
        </div>
      </div>
    </div>
  </div>

  <script>
    function switchTab(tab) {
      document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
      event.target.classList.add('active');
      document.getElementById('tab-code').style.display = tab === 'code' ? 'block' : 'none';
      document.getElementById('tab-url').style.display = tab === 'url' ? 'block' : 'none';
      document.getElementById('tab-contrast').style.display = tab === 'contrast' ? 'block' : 'none';
    }

    async function runAudit(type) {
      const resultsDiv = document.getElementById('results-view');
      resultsDiv.innerHTML = '<div style=""text-align: center; padding: 3rem;"">⏳ Running accessibility audit engine...</div>';

      const payload = {
        level: document.getElementById('wcag-level').value,
        html: type === 'code' ? document.getElementById('html-input').value : null,
        url: type === 'url' ? document.getElementById('url-input').value : null
      };

      try {
        const res = await fetch('/api/audit', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
        const data = await res.json();
        renderResults(data);
      } catch (err) {
        resultsDiv.innerHTML = '<div style=""color: #ef4444; padding: 2rem;"">❌ Audit failed: ' + err.message + '</div>';
      }
    }

    function renderResults(data) {
      const resultsDiv = document.getElementById('results-view');
      let scoreColor = data.accessibilityScore >= 90 ? '#10b981' : data.accessibilityScore >= 70 ? '#f59e0b' : '#ef4444';
      
      let html = `
        <div class=""score-banner"">
          <div>
            <div style=""font-size: 1.2rem; font-weight: bold;"">Score: ${data.accessibilityScore}/100 (Grade: ${data.grade})</div>
            <div style=""font-size: 0.85rem; color: #94a3b8;"">WCAG AA: ${data.passesWcagAa ? '✅ Pass' : '❌ Fail'} &bull; ${data.totalIssues} violations detected</div>
          </div>
          <div class=""score-circle"" style=""border: 3px solid ${scoreColor}; color: ${scoreColor};"">
            ${data.grade}
          </div>
        </div>
      `;

      if (data.issues.length === 0) {
        html += '<div style=""color: #10b981; padding: 2rem; text-align: center;"">🎉 No accessibility violations found! All checks passed.</div>';
      } else {
        data.issues.forEach((iss, idx) => {
          const sev = iss.severity.toLowerCase();
          html += `
            <div class=""issue-item ${sev}"">
              <div style=""display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem;"">
                <span class=""badge ${sev}"">${iss.severity}</span>
                <span style=""font-size: 0.75rem; color: #94a3b8;"">WCAG ${iss.criterion.number}</span>
              </div>
              <div style=""font-weight: 600; font-size: 0.95rem; margin-bottom: 0.3rem;"">[${iss.ruleId}] ${iss.ruleName}</div>
              <div style=""font-size: 0.85rem; color: #cbd5e1; margin-bottom: 0.4rem;"">${iss.message}</div>
              <div style=""font-size: 0.85rem; color: #10b981;"">💡 <strong>Fix:</strong> ${iss.remediationSummary}</div>
              ${iss.suggestedFixCode ? `<div class=""fix-box"">Suggested Code Fix:\n${iss.suggestedFixCode}</div>` : ''}
            </div>
          `;
        });
      }

      resultsDiv.innerHTML = html;
    }

    async function checkContrast() {
      const fg = document.getElementById('fg-input').value;
      const bg = document.getElementById('bg-input').value;
      const out = document.getElementById('contrast-output');

      try {
        const res = await fetch('/api/contrast', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ foreground: fg, background: bg })
        });
        const c = await res.json();
        out.innerHTML = `
          <div class=""contrast-box"">
            <div class=""contrast-preview"" style=""background-color: ${bg}; color: ${fg};"">Sample Text</div>
            <div>
              <div style=""font-size: 1.2rem; font-weight: bold;"">Ratio: ${c.ratio}:1 (${c.ratingString})</div>
              <div style=""font-size: 0.85rem; color: #94a3b8;"">
                WCAG Normal AA (4.5:1): ${c.passesNormalAa ? '✅ Pass' : '❌ Fail'}<br>
                WCAG Large AA (3.0:1): ${c.passesLargeAa ? '✅ Pass' : '❌ Fail'}<br>
                WCAG Normal AAA (7.0:1): ${c.passesNormalAaa ? '✅ Pass' : '❌ Fail'}
              </div>
            </div>
          </div>
        `;
      } catch (e) {
        out.innerHTML = '<div style=""color: #ef4444;"">Error calculating contrast</div>';
      }
    }
  </script>
</body>
</html>";
}

public record AuditRequest(string? Html, string? Url, string? Level);
public record ContrastRequest(string Foreground, string Background);
