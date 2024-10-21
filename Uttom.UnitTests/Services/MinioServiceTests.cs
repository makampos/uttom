using FluentAssertions;
using Uttom.Infrastructure.Services;
using Uttom.Infrastructure.TestData;

namespace Uttom.UnitTests.Services;

[Collection("Unit Tests")]
public class MinioServiceTests
{
    private readonly MinioService _minioService;

    public MinioServiceTests()
    {
        _minioService = new MinioService();
    }

    [Fact]
    public async Task UploadImageAsync_WithValidData_ShouldReturnObjectName()
    {
        // Arrange
        var delivererId = 1;
        var base64ImageData = ImageConverter.ConvertToBase64("cnh.png");

        // Act
        var objectName = await _minioService.UploadImageAsync(delivererId, base64ImageData);

        // Assert
        objectName.Should().NotBeNullOrEmpty();
        objectName.Should().StartWith(delivererId.ToString());
        objectName.Substring(objectName.LastIndexOf('.')).Should().Be(".png");
    }
}