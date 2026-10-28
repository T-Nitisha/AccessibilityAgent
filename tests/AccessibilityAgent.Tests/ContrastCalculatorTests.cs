using AccessibilityAgent.Core.Analysis;
using Xunit;

namespace AccessibilityAgent.Tests;

public class ContrastCalculatorTests
{
    [Fact]
    public void BlackOnWhite_HasMaximumContrast()
    {
        var result = ContrastCalculator.Calculate("#000000", "#ffffff");
        Assert.Equal(21.0, result.Ratio);
        Assert.True(result.PassesNormalAa);
        Assert.True(result.PassesNormalAaa);
    }

    [Fact]
    public void WhiteOnWhite_HasMinimumContrast()
    {
        var result = ContrastCalculator.Calculate("#ffffff", "#ffffff");
        Assert.Equal(1.0, result.Ratio);
        Assert.False(result.PassesNormalAa);
        Assert.False(result.PassesNormalAaa);
    }

    [Fact]
    public void NamedColors_AreParsedCorrectly()
    {
        var result = ContrastCalculator.Calculate("black", "white");
        Assert.Equal(21.0, result.Ratio);
    }

    [Fact]
    public void LowContrast_FailsAA()
    {
        // Light gray (#aaaaaa) on white (#ffffff) has ratio approx 2.32:1
        var result = ContrastCalculator.Calculate("#aaaaaa", "#ffffff");
        Assert.True(result.Ratio < 4.5);
        Assert.False(result.PassesNormalAa);
    }

    [Fact]
    public void RgbSyntax_IsParsedCorrectly()
    {
        var result = ContrastCalculator.Calculate("rgb(0, 0, 0)", "rgb(255, 255, 255)");
        Assert.Equal(21.0, result.Ratio);
    }
}
