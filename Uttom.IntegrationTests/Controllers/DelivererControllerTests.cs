using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Services;
using Uttom.Infrastructure.TestData;
using Uttom.IntegrationTests.Factories;
using Uttom.IntegrationTests.Helpers;

namespace Uttom.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class DelivererControllerTests : TestHelper, IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DelivererControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.InitializeAsync().GetAwaiter().GetResult();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateDeliverer_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);

        // Act
        var response = await _client.PostAsJsonAsync("/entregadores", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDeliverer_Should_Return_BadRequest_When_BusinessTaxId_Exists()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        response.EnsureSuccessStatusCode();

        var anotherCommandWithExistingBusinessTaxId = new AddDelivererCommand(
            "AD",
            "Another Deliverer",
            command.BusinessTaxId,
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);

        // Act
        var responseWithExistingBusinessTaxId = await _client.PostAsJsonAsync("/entregadores", anotherCommandWithExistingBusinessTaxId);
        var result = await responseWithExistingBusinessTaxId.Content.ReadAsStringAsync();

        // Assert
        responseWithExistingBusinessTaxId.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().Contain("The business tax id must be unique.");
    }

    [Fact]
    public async Task CreateDeliverer_Should_Return_BadRequest_When_DriverLicenseNumber_Exists()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        response.EnsureSuccessStatusCode();

        var anotherCommandWithExistingDriverLicenseNumber = new AddDelivererCommand(
            "AD",
            "Another Deliverer",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            command.DriverLicenseNumber,
            1,
            null);

        // Act
        var responseWithExistingDriverLicenseNumber = await _client.PostAsJsonAsync("/entregadores", anotherCommandWithExistingDriverLicenseNumber);
        var result = await responseWithExistingDriverLicenseNumber.Content.ReadAsStringAsync();

        // Assert
        responseWithExistingDriverLicenseNumber.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().Contain("The driver license number must be unique.");
    }

    // TODO: Add validator to check if the image extension is valid
    [Fact]
    public async Task CreateDeliverer_Should_Return_BadRequest_When_Invalid_Image_Extension()
    {
        // Arrange
        var base64ImageData = ImageConverter.ConvertToBase64("cnh.jpeg");
        var command = new AddDelivererCommand(
            "mm",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            base64ImageData);

        // Act
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().Contain("The image extension is not valid.");
    }

    [Fact]
    public async Task CreateDeliverer_Should_Upload_Image_To_Storage()
    {
        // Arrange
        var base64ImageData = ImageConverter.ConvertToBase64("cnh.png");
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            base64ImageData);

        // Act
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().BeEmpty();

        var scope = _factory.Server.Services.CreateScope();
        var deliverer = await scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>()
            .DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId);

        scope.ServiceProvider.GetRequiredService<IMinioService>().GetImageAsync(deliverer!.DriverLicenseImageId!)
            .Should().NotBeNull();
    }

    [Fact]
    public async Task UploadDriverLicenseImage_Should_Return_Created_When_Successful()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        response.EnsureSuccessStatusCode();

        var deliverer = await _factory.Server.Services.CreateScope().ServiceProvider.GetRequiredService<IUttomUnitOfWork>()
            .DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId);

        var driverLicenseCommand = new AddOrUpdateDriverLicenseCommand(ImageConverter.ConvertToBase64("cnh.png"), deliverer!.Id);

        // Act
        var responseUploadImage = await _client.PostAsJsonAsync($"/entregadores/{deliverer.Id}/cnh", driverLicenseCommand);

        // Assert
        responseUploadImage.EnsureSuccessStatusCode();
        responseUploadImage.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UploadDriverLicenseImage_Should_Return_BadRequest_When_Invalid_Image_Extension()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);
        var response = await _client.PostAsJsonAsync("/entregadores", command);
        response.EnsureSuccessStatusCode();

        var deliverer = await _factory.Server.Services.CreateScope().ServiceProvider.GetRequiredService<IUttomUnitOfWork>()
            .DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId);

        var driverLicenseCommand = new AddOrUpdateDriverLicenseCommand(ImageConverter.ConvertToBase64("cnh.jpeg"), deliverer!.Id);

        // Act
        var responseUploadImage = await _client.PostAsJsonAsync($"/entregadores/{deliverer.Id}/cnh", driverLicenseCommand);
        var result = await responseUploadImage.Content.ReadAsStringAsync();

        // Assert
        responseUploadImage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().Contain("The image extension is not valid.");
    }
}