using AccessibilityAgent.Core.Engine;
using AccessibilityAgent.Core.Models;
using Xunit;

namespace AccessibilityAgent.Tests;

public class RuleTests
{
    private readonly AccessibilityEngine _engine = new();

    [Fact]
    public async Task ImgWithoutAlt_FlagsCriticalIssue()
    {
        var html = @"<div><img src=""photo.jpg""></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "image-alt" && i.Severity == IssueSeverity.Critical);
    }

    [Fact]
    public async Task ImgWithDescriptiveAlt_Passes()
    {
        var html = @"<div><img src=""photo.jpg"" alt=""A golden retriever running on grass""></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.DoesNotContain(result.Issues, i => i.RuleId == "image-alt");
    }

    [Fact]
    public async Task ImgWithSuspiciousAlt_FlagsModerateIssue()
    {
        var html = @"<div><img src=""photo.jpg"" alt=""image""></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "image-alt" && i.Severity == IssueSeverity.Moderate);
    }

    [Fact]
    public async Task HtmlWithoutLang_FlagsCriticalIssue()
    {
        var html = @"<!DOCTYPE html><html><head><title>Test</title></head><body><h1>Hello</h1></body></html>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "html-has-lang" && i.Severity == IssueSeverity.Critical);
    }

    [Fact]
    public async Task ButtonWithoutTextOrAria_FlagsCriticalIssue()
    {
        var html = @"<button><i class=""fa fa-trash""></i></button>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "button-name" && i.Severity == IssueSeverity.Critical);
    }

    [Fact]
    public async Task ButtonWithAriaLabel_Passes()
    {
        var html = @"<button aria-label=""Delete item""><i class=""fa fa-trash""></i></button>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.DoesNotContain(result.Issues, i => i.RuleId == "button-name");
    }

    [Fact]
    public async Task InputWithoutLabel_FlagsCriticalIssue()
    {
        var html = @"<form><input type=""text"" name=""first_name""></form>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "form-input-label" && i.Severity == IssueSeverity.Critical);
    }

    [Fact]
    public async Task InputWithMatchingLabel_Passes()
    {
        var html = @"<form><label for=""fname"">First Name</label><input type=""text"" id=""fname"" name=""first_name""></form>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.DoesNotContain(result.Issues, i => i.RuleId == "form-input-label");
    }

    [Fact]
    public async Task PositiveTabIndex_FlagsSeriousIssue()
    {
        var html = @"<a href=""#"" tabindex=""3"">Jump</a>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "tabindex-positive" && i.Severity == IssueSeverity.Serious);
    }

    [Fact]
    public async Task InvalidAriaRole_FlagsSeriousIssue()
    {
        var html = @"<div role=""custom-btn"">Click me</div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "aria-valid-role");
    }

    [Fact]
    public async Task AriaHiddenWithFocusableElement_FlagsCriticalIssue()
    {
        var html = @"<div aria-hidden=""true""><button>Clickable</button></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "aria-hidden-focusable" && i.Severity == IssueSeverity.Critical);
    }

    [Fact]
    public async Task SkippedHeadingLevel_FlagsModerateIssue()
    {
        var html = @"<div><h1>Title</h1><h4>Sub section</h4></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "heading-order" && i.Severity == IssueSeverity.Moderate);
    }

    [Fact]
    public async Task AmbiguousLinkText_FlagsModerateIssue()
    {
        var html = @"<div><a href=""/about"">Click here</a></div>";
        var result = await _engine.AuditHtmlAsync(html);

        Assert.Contains(result.Issues, i => i.RuleId == "link-text" && i.Severity == IssueSeverity.Moderate);
    }
}
