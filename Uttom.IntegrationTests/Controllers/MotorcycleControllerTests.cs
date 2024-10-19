using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Messages;
using Uttom.Domain.Results;
using Uttom.IntegrationTests.Fixtures;

namespace Uttom.IntegrationTests.Controllers;

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
        var command = new AddMotorcycleCommand("ID123", 2024, "ModelX", "ABC123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/motorcycles", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultResponse<bool>>();

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        var message = await CheckMessageConsumedAsync();


        message.Items.Should().HaveCount(1);
    }


    //TODO: Refactor to get message by plate number
    private async Task<PagedResult<RegisteredMotorcycle>> CheckMessageConsumedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        var message = await uttomUnitOfWork.RegisteredMotorCyclesRepository.GetAllAsync(1,1);
        return message;
    }
}