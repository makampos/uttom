using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers;

public class AddRentalCommandHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly AddRentalCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public AddRentalCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _motorcycleRepository = new MotorcycleRepository(_dbContext);
        _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
        _delivererRepository = new DelivererRepository(_dbContext);
        _rentalRepository = new RentalRepository(_dbContext);

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext, _motorcycleRepository, _registeredMotorCycleRepository, _delivererRepository, _rentalRepository);

        _handler = new AddRentalCommandHandler(_uttomUnitOfWork);
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
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
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
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", "20.681.653/0001-9",
            new DateTime(1992, 10, 20), "59375336842", DriverLicenseType.A);
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
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", "20.681.653/0001-9",
            new DateTime(1992, 10, 20), "59375336842", DriverLicenseType.B);
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
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", "20.681.653/0001-9",
            new DateTime(1992, 10, 20), "59375336842", DriverLicenseType.A);
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