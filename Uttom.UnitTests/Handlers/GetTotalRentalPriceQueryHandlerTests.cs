using FluentAssertions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class GetTotalRentalPriceQueryHandlerTests : BaseTestHandler<GetTotalRentalPriceQueryHandler>
{
    private readonly GetTotalRentalPriceQueryHandler _handler;
    public GetTotalRentalPriceQueryHandlerTests()
    {
        _handler = CreateHandler(_uttomUnitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturn_TotalPrice_WhenRentalIsFound()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);

        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var endDate = command.StartDate.AddDays(RentalPlans.GetPlan(7)!.Days);

        var rentalEntity = Rental.Create(7,
            endDate,
            command.EstimatingEndingDate,
            existingDeliverer.Id,
            existingMotorcycle.Id);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.RentalRepository.AddAsync(rentalEntity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var actualReturnDate = endDate.AddDays(-2);
        var query = new GetTotalRentalPriceQuery(rentalEntity.Id, actualReturnDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(162m);
    }
}