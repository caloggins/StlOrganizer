using System.Globalization;
using Shouldly;
using StlOrganizer.Gui.Converters;

namespace StlOrganizer.Gui.Tests.Converters;

public class InverseBooleanConverterTests
{
    private readonly InverseBooleanConverter converter = new();
    private readonly CultureInfo culture = CultureInfo.InvariantCulture;

    [Fact]
    public void Convert_WithTrue_ReturnsFalse()
    {
        var result = converter.Convert(true, typeof(bool), null, culture);

        result.ShouldBe(false);
    }

    [Fact]
    public void Convert_WithFalse_ReturnsTrue()
    {
        var result = converter.Convert(false, typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void Convert_WithNull_ReturnsTrue()
    {
        var result = converter.Convert(null, typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void Convert_WithNonBooleanValue_ReturnsTrue()
    {
        var result = converter.Convert("not a boolean", typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void Convert_WithInteger_ReturnsTrue()
    {
        var result = converter.Convert(42, typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void ConvertBack_WithTrue_ReturnsFalse()
    {
        var result = converter.ConvertBack(true, typeof(bool), null, culture);

        result.ShouldBe(false);
    }

    [Fact]
    public void ConvertBack_WithFalse_ReturnsTrue()
    {
        var result = converter.ConvertBack(false, typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void ConvertBack_WithNull_ReturnsTrue()
    {
        var result = converter.ConvertBack(null, typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void ConvertBack_WithNonBooleanValue_ReturnsTrue()
    {
        var result = converter.ConvertBack("not a boolean", typeof(bool), null, culture);

        result.ShouldBe(true);
    }

    [Fact]
    public void Convert_IsSymmetric()
    {
        var originalValue = true;

        var converted = converter.Convert(originalValue, typeof(bool), null, culture);
        var convertedBack = converter.ConvertBack(converted, typeof(bool), null, culture);

        convertedBack.ShouldBe(originalValue);
    }

    [Fact]
    public void Convert_WithDifferentCulture_WorksCorrectly()
    {
        var germanCulture = new CultureInfo("de-DE");

        var result = converter.Convert(true, typeof(bool), null, germanCulture);

        result.ShouldBe(false);
    }

    [Fact]
    public void ConvertBack_WithDifferentCulture_WorksCorrectly()
    {
        var frenchCulture = new CultureInfo("fr-FR");

        var result = converter.ConvertBack(true, typeof(bool), null, frenchCulture);

        result.ShouldBe(false);
    }

    [Fact]
    public void Convert_WithParameter_IgnoresParameter()
    {
        var result = converter.Convert(true, typeof(bool), "some parameter", culture);

        result.ShouldBe(false);
    }

    [Fact]
    public void ConvertBack_WithParameter_IgnoresParameter()
    {
        var result = converter.ConvertBack(false, typeof(bool), "some parameter", culture);

        result.ShouldBe(true);
    }
}
