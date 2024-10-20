using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Messages;
using Uttom.Domain.Models;
using Uttom.Domain.Results;
using Uttom.IntegrationTests.Fixtures;

namespace Uttom.IntegrationTests.Controllers;

//TODO: Revisit these tests to adjust routes and responses later

public class MotorcycleControllerTests : IClassFixture<CustomWebApplicationFactory>
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
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");

        // Act
        var response = await _client.PostAsJsonAsync("/api/motorcycles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMotorcycle_Should_Publish_Message_When_Year_Is_2024()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2024, "ModelX", "ABC-1234");

        // Act
        var response = await _client.PostAsJsonAsync("/api/motorcycles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var message = await CheckMessageConsumedAsync();
        message.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllMotorcycles_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcyclesQuery(1, 10);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        var result = await response.Content.ReadFromJsonAsync<ResultResponse<PagedResult<Motorcycle>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
    }


    [Fact]
    public async Task GetAllMotorcycles_Should_Return_Ok_When_Empty()
    {
        // Arrange
        var query = new GetMotorcyclesQuery(1, 10);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles?pageNumber={query.PageNumber}&pageSize={query.PageSize}");
        var result = await response.Content.ReadFromJsonAsync<ResultResponse<PagedResult<Motorcycle>>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetMotorcycleByPlateNumber_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}");
        var result = await response.Content.ReadFromJsonAsync<Motorcycle>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.PlateNumber.Should().Be(query.PlateNumber);
    }

    [Fact]
    public async Task GetMotorcycleByPlateNumber_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        var query = new GetMotorcycleByPlateNumberQuery("ABC-1234");

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
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        var query = new GetMotorcycleByPlateNumberQuery(command.PlateNumber);

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/plate-number?plateNumber={query.PlateNumber}"); // this is wierd LOL
        var result = await response.Content.ReadFromJsonAsync<Motorcycle>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.PlateNumber.Should().Be(query.PlateNumber);
    }

    [Fact]
    public async Task GetMotorcycleById_Should_Return_Ok_When_Found()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/motorcycles/{1}"); // Make it better
        var result = await response.Content.ReadFromJsonAsync<Motorcycle>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetMotorcycleById_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"/api/motorcycles/{1}"); // Make it better
        var result = await response.Content.ReadAsStringAsync();
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task DeleteMotorcycle_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var command = new AddMotorcycleCommand("ID1234", 2023, "ModelX", "ABC-1234");
        var motorcycleIsCreated = await _client.PostAsJsonAsync("/api/motorcycles", command);
        motorcycleIsCreated.EnsureSuccessStatusCode();

        // Act
        var response = await _client.DeleteAsync($"/api/motorcycles/{1}"); // Make it better

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteMotorcycle_Should_Return_NotFound_When_Empty()
    {
        // Arrange
        // Act
        var response = await _client.DeleteAsync($"/api/motorcycles/{1}"); // Make it better
        var result = await response.Content.ReadAsStringAsync();
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().Be("Motorcycle not found.");
    }

    //TODO: Add test for when motorcycle has rental record


    //TODO: Refactor to get message by plate number
    private async Task<PagedResult<RegisteredMotorcycle>> CheckMessageConsumedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        var message = await uttomUnitOfWork.RegisteredMotorCyclesRepository.GetAllAsync(1,1);
        return message;
    }
}