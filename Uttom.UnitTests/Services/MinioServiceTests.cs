using FluentAssertions;
using Uttom.Application.Extensions;
using Uttom.Infrastructure.Services;

namespace Uttom.UnitTests.Services;

[Collection("Unit Tests")]
public class MinioServiceTests
{
    private readonly MinioService _minioService;
    private const string PATH = "TestData/Images";

    public MinioServiceTests()
    {
        _minioService = new MinioService();
    }

    [Fact]
    public async Task UploadImageAsync_WithValidData_ShouldReturnObjectName()
    {
        // Arrange
        var delivererId = 1;
        var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.png");

        // Act
        var objectName = await _minioService.UploadImageAsync(delivererId, base64ImageData);

        // Assert
        objectName.Should().NotBeNullOrEmpty();
        objectName.Should().StartWith(delivererId.ToString());
        objectName.Substring(objectName.LastIndexOf('.')).Should().Be(".png");
    }
}