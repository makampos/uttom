using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Services;
using Uttom.IntegrationTests.Fixtures;

namespace Uttom.IntegrationTests.Controllers;

public class DelivererControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private const string PATH = "TestData/Images";

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
            "20.681.653/0001-90",
            new DateTime(1990, 1, 1),
            "123456789",
            1,
            null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/deliverers", command);

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
            "20.681.653/0001-90",
            new DateTime(1990, 1, 1),
            "123456789",
            1,
            null);
        var response = await _client.PostAsJsonAsync("/api/deliverers", command);
        response.EnsureSuccessStatusCode();

        var anotherCommandWithExistingBusinessTaxId = new AddDelivererCommand(
            "AD",
            "Another Deliverer",
            command.BusinessTaxId,
            new DateTime(1990, 1, 1),
            "22345678900",
            1,
            null);

        // Act
        var responseWithExistingBusinessTaxId = await _client.PostAsJsonAsync("/api/deliverers", anotherCommandWithExistingBusinessTaxId);
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
            "20.681.653/0001-90",
            new DateTime(1990, 1, 1),
            "123456789",
            1,
            null);
        var response = await _client.PostAsJsonAsync("/api/deliverers", command);
        response.EnsureSuccessStatusCode();

        var anotherCommandWithExistingDriverLicenseNumber = new AddDelivererCommand(
            "AD",
            "Another Deliverer",
            "31.121.666/0001-40",
            new DateTime(1990, 1, 1),
            command.DriverLicenseNumber,
            1,
            null);

        // Act
        var responseWithExistingDriverLicenseNumber = await _client.PostAsJsonAsync("/api/deliverers", anotherCommandWithExistingDriverLicenseNumber);
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
        var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.jpeg");
        var command = new AddDelivererCommand(
            "mm",
            "Matheus",
            "31.121.666/0001-40",
            new DateTime(1990, 1, 1),
            "12345678900",
            1,
            base64ImageData);

        // Act
        var response = await _client.PostAsJsonAsync("/api/deliverers", command);
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().Contain("The image extension is not valid.");
    }

    [Fact]
    public async Task CreateDeliverer_Should_Upload_Image_To_Storage()
    {
        // Arrange
        var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.png");
        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            "31.121.666/0001-40",
            new DateTime(1990, 1, 1),
            "12345678900",
            1,
            base64ImageData);

        // Act
        var response = await _client.PostAsJsonAsync("/api/deliverers", command);
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().BeEmpty();

        var scope = _factory.Server.Services.CreateScope();
        var deliverer = await scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>()
            .DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId);

        scope.ServiceProvider.GetRequiredService<IMinioService>().GetImageAsync(deliverer!.DriverLicenseImageId).Should().NotBeNull();
    }
}