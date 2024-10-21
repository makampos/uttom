using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Uttom.Application.DTOs;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
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

        var motorcycleId = await _factory.GetMotorcycle(motorcycleCommand.PlateNumber, _factory);
        var delivererId = await _factory.GetDeliverer(delivererCommand.BusinessTaxId, _factory);


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

        var motorcycle = await _factory.GetMotorcycle(motorcycleCommand.PlateNumber, _factory);
        var deliverer = await _factory.GetDeliverer(delivererCommand.BusinessTaxId, _factory);

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


        var motorcycle = await _factory.GetMotorcycle(motorcycleCommand.PlateNumber, _factory);
        var deliverer = await _factory.GetDeliverer(delivererCommand.BusinessTaxId, _factory);

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

    [Fact]
    public async Task GetRentalPrice_Should_Return_Ok_When_Successful()
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

        var motorcycleId = await _factory.GetMotorcycle(motorcycleCommand.PlateNumber, _factory);
        var delivererId = await _factory.GetDeliverer(delivererCommand.BusinessTaxId, _factory);

        var command = new AddRentalCommand(
            PlanId: RentalPlans.GetPlan(7)!.Days,
            DeliverId: delivererId.Id,
            MotorcycleId: motorcycleId.Id,
            StartDate: DateOnly.FromDateTime(DateTime.Today),
            EstimatingEndingDate: DateOnly.FromDateTime(DateTime.Today.AddDays(8)));

        var response = await _client.PostAsJsonAsync("/locacao", command);

        response.EnsureSuccessStatusCode();

        var existingRental = await _factory.GetRentalByDelivererId(delivererId.Id, _factory);
        var query = new GetTotalRentalPriceQueryString(DateOnly.FromDateTime(DateTime.Today.AddDays(10)));

        var rental = await _client.GetAsync($"/locacao/{existingRental!.Id}/devolucao?DataDevolucao={query.DataDevolucao:yyyy-MM-dd}");

        rental.EnsureSuccessStatusCode();
        rental.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}