using FluentAssertions;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class GetRentalQueryHandlerTests : BaseTestHandler<GetRentalQueryHandler>
{
    private readonly GetRentalQueryHandler _handler;

    public GetRentalQueryHandlerTests()
    {
        _handler = CreateHandler(_uttomUnitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenRentalDoesNotExist()
    {
        // Arrange
        var request = new GetRentalQuery(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Rental not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnRental_WhenRentalExists()
    {
        // Arrange
        var motorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var deliverer = Deliverer.Create(
            "SEA",
            "Sara Elza Alves",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20),
            GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.AB);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(motorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(deliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var rental = Rental.Create(
            7,
            DateOnly.FromDateTime(DateTime.Now.AddDays(RentalPlans.GetPlan(7)!.Days)),
            DateOnly.FromDateTime(DateTime.Now.AddDays(8))  ,
            motorcycle.Id,
            motorcycle.Id);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rental);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetRentalQuery(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}