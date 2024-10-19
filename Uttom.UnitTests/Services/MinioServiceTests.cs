using FluentAssertions;
using Uttom.Application.Extensions;
using Uttom.Infrastructure.Services;

namespace Uttom.UnitTests.Services;

//TODO: Crete integration tests or MOQ using NSubstitute
public class MinioServiceTests
{
    private readonly MinioService _minioService;
    private const string PATH = "TestData/Images";

    public MinioServiceTests()
    {
        _minioService = new MinioService();
    }

    // Create a test to upload image
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