using FluentAssertions;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Services;
using Uttom.Infrastructure.TestData;

namespace Uttom.UnitTests.Services;

[Collection("Unit Tests")]
public class ImageServiceTests
{
    private readonly IImageService _imageService = new ImageService();

    [Fact]
    public void ValidateImageExtension_ShouldReturnFalse_WhenBase64ImageDataIsNull()
    {
        // Arrange
        var base64ImageData = string.Empty;

        // Act
        var result = _imageService.ValidateImageExtension(base64ImageData);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateImageExtension_ShouldReturnFalse_WhenBase64ImageDataIsNotBase64String()
    {
        // Arrange
        var base64ImageData = "not a base64 string";

        // Act
        var result = _imageService.ValidateImageExtension(base64ImageData);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("cnh.jpeg")]
    public void ValidateImageExtension_ShouldReturnFalse_WhenBase64ImageDataIsNotPngOrBmp(string fileName)
    {
        // Arrange
        var base64ImageData = ImageConverter.ConvertToBase64(fileName);

        // Act
        var result = _imageService.ValidateImageExtension(base64ImageData);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("cnh.png")]
    [InlineData("cnh.bmp")]
    public void ValidateImageExtension_ShouldReturnTrue_WhenBase64ImageDataIsValid(string fileName)
    {
        // Arrange
        var base64ImageData = ImageConverter.ConvertToBase64(fileName);

        // Act
        var result = _imageService.ValidateImageExtension(base64ImageData);

        // Assert
        result.Should().BeTrue();
    }
}