using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddRentalCommandHandlerTests : BaseTestHandler<AddRentalCommandHandler>
{
    private readonly AddRentalCommandHandler _handler;

    public AddRentalCommandHandlerTests()
    {
        _handler = CreateHandler(parameters: new object[] { _uttomUnitOfWork, _logger });
    }

    [Fact]
    public async Task Handle_WhenPlanNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new AddRentalCommand(88,
            1,
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Plan not found.");
    }

    [Fact]
    public async Task Handle_WhenDelivererNotFound_ReturnsFailureResult()
    {
        // Arrange
        // Add motorcycle to he database
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        await _dbContext.Motorcycles.AddAsync(existingMotorcycle);
        await _dbContext.SaveChangesAsync();

        var command = new AddRentalCommand(7,
            1,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Deliverer not found.");
    }

    [Fact]
    public async Task Handle_WhenMotorcycleNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new AddRentalCommand(7,
            1,
            1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_WhenStartDateLessThanToday_ReturnsFailureResult()
    {
        // Arrange
        // Add motorcycle to he database
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);
        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today).AddDays(-1),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The start date must be today or a future date.");
    }

    [Fact]
    public async Task Handle_WhenDelivererHasDriverLicenseTypeB_ReturnsFailureResult()
    {
        // Add motorcycle to he database
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.B);
        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        // Act
        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Deliverer must have a driver license type A.");
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsSuccessResult()
    {
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);
        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        // Act
        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        var rental = await _dbContext.Rentals.FirstOrDefaultAsync(x => x.DelivererId == existingDeliverer.Id);

        rental.Should().NotBeNull();
        rental.PlanId.Should().Be(7);
        rental.DelivererId.Should().Be(existingDeliverer.Id);
        rental.MotorcycleId.Should().Be(existingMotorcycle.Id);
        rental.StartDate.Should().Be(DateOnly.FromDateTime(DateTime.Today).AddDays(1)); // next day
        rental.EndDate.Should().Be(DateOnly.FromDateTime(DateTime.Today).AddDays(7)); // 7 days rental counting by the start date
        rental.EstimatingEndingDate.Should().Be(DateOnly.FromDateTime(DateTime.Today).AddDays(10));
    }
}