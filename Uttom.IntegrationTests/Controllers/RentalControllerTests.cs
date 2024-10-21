using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Uttom.Application.DTOs;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.IntegrationTests.Factories;
using Uttom.IntegrationTests.Helpers;

namespace Uttom.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class RentalControllerTests : TestHelper, IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RentalControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.InitializeAsync().GetAwaiter().GetResult();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateRental_Should_Return_Ok_When_Successful()
    {
        // Arrange
        // add motorcycle
        var motorcycleCommand = new AddMotorcycleCommand(
            Identifier: "ID1234",
            Year: 2023,
            Model: "ModelX",
            PlateNumber: GeneratePlateNumber());

        var motorcycleResponse = await _client.PostAsJsonAsync("/motos", motorcycleCommand);
        motorcycleResponse.EnsureSuccessStatusCode();

        // add deliverer
        var delivererCommand = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);

        var delivererResponse = await _client.PostAsJsonAsync("/entregadores", delivererCommand);
        delivererResponse.EnsureSuccessStatusCode();

        var motorcycleId = await GetMotorcycle(motorcycleCommand.PlateNumber);
        var delivererId = await GetDeliverer(delivererCommand.BusinessTaxId);


        var command = new AddRentalCommand(
            PlanId: RentalPlans.GetPlan(7)!.Days,
            DeliverId: delivererId.Id,
            MotorcycleId: motorcycleId.Id,
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            EstimatingEndingDate: DateOnly.FromDateTime(DateTime.Today.AddDays(8)));

        // Act
        var response = await _client.PostAsJsonAsync("/locacao", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetRentalById_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var motorcycleCommand = new AddMotorcycleCommand(
            Identifier: "ID1234",
            Year: 2023,
            Model: "ModelX",
            PlateNumber: GeneratePlateNumber());

        var motorcycleResponse = await _client.PostAsJsonAsync("/motos", motorcycleCommand);
        motorcycleResponse.EnsureSuccessStatusCode();

        var delivererCommand = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);

        var delivererResponse = await _client.PostAsJsonAsync("/entregadores", delivererCommand);
        delivererResponse.EnsureSuccessStatusCode();

        var motorcycle = await GetMotorcycle(motorcycleCommand.PlateNumber);
        var deliverer = await GetDeliverer(delivererCommand.BusinessTaxId);

        var command = new AddRentalCommand(
            PlanId: RentalPlans.GetPlan(7)!.Days,
            DeliverId: deliverer.Id,
            MotorcycleId: motorcycle.Id,
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            EstimatingEndingDate: DateOnly.FromDateTime(DateTime.Today.AddDays(8)));

        var response = await _client.PostAsJsonAsync("/locacao", command);
        response.EnsureSuccessStatusCode();


        // Act
        var rental = await _client.GetAsync($"/locacao/{1}");
        rental.EnsureSuccessStatusCode();

        var result = await rental.Content.ReadFromJsonAsync<RentalDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddReturnDate_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var motorcycleCommand = new AddMotorcycleCommand(
            Identifier: "ID1234",
            Year: 2023,
            Model: "ModelX",
            PlateNumber: "ABC-1234");

        var motorcycleResponse = await _client.PostAsJsonAsync("/motos", motorcycleCommand);
        motorcycleResponse.EnsureSuccessStatusCode();

        var delivererCommand = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1990, 1, 1),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            1,
            null);

        var delivererResponse = await _client.PostAsJsonAsync("/entregadores", delivererCommand);
        delivererResponse.EnsureSuccessStatusCode();

        var motorcycle = await GetMotorcycle(motorcycleCommand.PlateNumber);
        var deliverer = await GetDeliverer(delivererCommand.BusinessTaxId);

        var command = new AddRentalCommand(
            PlanId: RentalPlans.GetPlan(7)!.Days,
            DeliverId: deliverer.Id,
            MotorcycleId: motorcycle.Id,
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            EstimatingEndingDate: DateOnly.FromDateTime(DateTime.Today.AddDays(8)));

        var response = await _client.PostAsJsonAsync("/locacao", command);
        response.EnsureSuccessStatusCode();

        var rental = await _client.GetAsync($"/locacao/{1}");
        rental.EnsureSuccessStatusCode();

        var rentalDTo = await rental.Content.ReadFromJsonAsync<RentalDto>();

        var updateCommand = new UpdateRentalCommand(rentalDTo!.EndDate);

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/locacao/{rentalDTo.Id}/devolucao", updateCommand);
        var updateResult = await updateResponse.Content.ReadAsStringAsync();

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResult.Should().NotBeNullOrEmpty();
    }

    private async Task<Motorcycle> GetMotorcycle(string plateNumber)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        return await uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(plateNumber, false);
    }

    private async Task<Deliverer> GetDeliverer(string businessTaxId)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        return await uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(businessTaxId);
    }
}