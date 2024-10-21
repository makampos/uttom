using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Messages;
using Uttom.Domain.Results;
using Uttom.IntegrationTests.Factories;
using Uttom.IntegrationTests.Helpers;

namespace Uttom.IntegrationTests.Controllers;

//TODO: Revisit these tests to adjust routes and responses later

[Collection("Integration Tests")]
public class MotorcycleControllerTests : TestHelper, IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MotorcycleControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.InitializeAsync().GetAwaiter().GetResult();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateMotorcycle_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelN", GeneratePlateNumber());

        // Act
        var response = await _client.PostAsJsonAsync("/api/motorcycles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMotorcycle_Should_Publish_Message_When_Year_Is_2024()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2024, "ModelZ", GeneratePlateNumber());

        // Act
        var response = await _client.PostAsJsonAsync("/api/motorcycles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var message = await CheckMessageConsumedAsync(command.PlateNumber);
        message.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMotorcycles_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelJ", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcyclesQuery(1, 10);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        var result = await response.Content.ReadFromJsonAsync<ResultResponse<PagedResult<MotorcycleDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Success.Should().BeTrue();
        result.Data.Items.Should().NotBeEmpty();
    }


    [Fact(Skip = "Conflicting with other tests in parallel")]
    public async Task GetAllMotorcycles_Should_Return_Ok_When_Empty()
    {
        // Arrange
        var query = new GetMotorcyclesQuery(1, 10);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        var result = await response.Content.ReadFromJsonAsync<ResultResponse<PagedResult<MotorcycleDto>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Success.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMotorcycleByPlateNumber_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelH", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}");
        var result = await response.Content.ReadFromJsonAsync<MotorcycleDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.PlateNumber.Should().Be(query.PlateNumber);
    }

    [Fact]
    public async Task GetMotorcycleByPlateNumber_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        var query = new GetMotorcycleByPlateNumberQuery(GeneratePlateNumber());

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}"); // this is wierd LOL
        var result = await response.Content.ReadAsStringAsync();
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().Be("Motorcycle not found.");
    }

    // add success test when found
    [Fact]
    public async Task GetMotorcycleByPlateNumber_Should_Return_Ok_When_Found()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelZ", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}"); // this is wierd LOL
        var result = await response.Content.ReadFromJsonAsync<MotorcycleDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.PlateNumber.Should().Be(query.PlateNumber);
    }

    [Fact]
    public async Task GetMotorcycleById_Should_Return_Ok_When_Found()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);
        var motorcycleByPlateNumberResult = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}");
        var motorcycleByPlateNumber = await motorcycleByPlateNumberResult.Content.ReadFromJsonAsync<MotorcycleDto>();

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/{motorcycleByPlateNumber!.Id}");
        var result = await response.Content.ReadFromJsonAsync<MotorcycleDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMotorcycleById_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"/api/motorcycles/{100}");
        var result = await response.Content.ReadAsStringAsync();
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task DeleteMotorcycle_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelY", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        // Act
        var response = await _client.DeleteAsync($"/api/motorcycles/{1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteMotorcycle_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        // Act
        var response = await _client.DeleteAsync($"/api/motorcycles/{99}");
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var expectedErrorMessage = JsonSerializer.Serialize(new { message = "Motorcycle not found." });
        result.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task UpdateMotorcyclePlateNumber_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelY", GeneratePlateNumber());
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        // get motorcycle by plate number
        var motorcycleExists = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={command.PlateNumber}");
        var motorcycleExistsResult = await motorcycleExists.Content.ReadFromJsonAsync<MotorcycleDto>();

        var updateCommand = new UpdateMotorcycleCommand(GeneratePlateNumber(), motorcycleExistsResult!.Id);

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);
        var motorcycleByPlateNumberUpdatedExists = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}");
        var motorcycleByPlateNumberUpdatedResult = await motorcycleByPlateNumberUpdatedExists.Content.ReadFromJsonAsync<MotorcycleDto>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/motorcycles/{motorcycleByPlateNumberUpdatedResult!.Id}/plate-number", updateCommand);
        var result = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be("Motorcycle updated successfully.");
    }


    //TODO: Add test for when motorcycle has rental record

    private async Task<RegisteredMotorcycleDto> CheckMessageConsumedAsync(string plateNumber)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();

        var message = null as RegisteredMotorcycle;

        while (message == null)
        {
            await Task.Delay(1000);
            message = await uttomUnitOfWork.RegisteredMotorCyclesRepository.GetByPlateNumberAsync(plateNumber);
        }

        return message.ToDto();
    }
}