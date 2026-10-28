using AccessibilityAgent.Core.Engine;
using AccessibilityAgent.Core.Reporters;
using Xunit;

namespace AccessibilityAgent.Tests;

public class ReporterTests
{
    private readonly AccessibilityEngine _engine = new();

    [Fact]
    public async Task MarkdownReporter_GeneratesPrReviewFormat()
    {
        var html = @"<div><img src=""test.jpg""><button></button></div>";
        var result = await _engine.AuditHtmlAsync(html, "TestView.html");

        var reporter = new MarkdownReporter();
        var markdown = await reporter.GenerateReportAsync(result);

        Assert.Contains("# ♿ Accessibility Agent Audit Report", markdown);
        Assert.Contains("image-alt", markdown);
        Assert.Contains("button-name", markdown);
        Assert.Contains("Reviewer Accessibility Checklist", markdown);
    }

    [Fact]
    public async Task HtmlReporter_GeneratesValidHtmlDashboard()
    {
        var html = @"<div><img src=""test.jpg""></div>";
        var result = await _engine.AuditHtmlAsync(html, "TestView.html");

        var reporter = new HtmlReporter();
        var reportHtml = await reporter.GenerateReportAsync(result);

        Assert.Contains("<!DOCTYPE html>", reportHtml);
        Assert.Contains("Accessibility Audit Report", reportHtml);
        Assert.Contains("image-alt", reportHtml);
    }

    [Fact]
    public async Task SarifReporter_GeneratesValidSarif210Json()
    {
        var html = @"<div><img src=""test.jpg""></div>";
        var result = await _engine.AuditHtmlAsync(html, "TestView.html");

        var reporter = new SarifReporter();
        var sarif = await reporter.GenerateReportAsync(result);

        Assert.Contains("\"version\": \"2.1.0\"", sarif);
        Assert.Contains("\"ruleId\": \"image-alt\"", sarif);
    }
}
