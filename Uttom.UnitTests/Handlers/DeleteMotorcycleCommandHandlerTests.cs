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

public class DeleteMotorcycleCommandHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteMotorCycleCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public DeleteMotorcycleCommandHandlerTests()
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

        _handler = new DeleteMotorCycleCommandHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleIsNotFound()
    {
        // Arrange
        var request = new DeleteMotorcycleCommand(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    // Add fail test for when motorcycle has rental record
    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleHasRentalRecord()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", "20.681.653/0001-9",
            new DateTime(1992, 10, 20), "59375336842", DriverLicenseType.A);
        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var rental = Rental.Create(7, DateOnly.FromDateTime(DateTime.Today).AddDays(7),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10), existingDeliverer.Id, existingMotorcycle.Id);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rental);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new DeleteMotorcycleCommand(existingMotorcycle.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle has rental record.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenMotorcycleIsDeleted()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", "DHA-1234");

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new DeleteMotorcycleCommand(entity.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        var deletedEntity = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(entity.PlateNumber, true);

        deletedEntity.Should().NotBeNull();
        deletedEntity.IsDeleted.Should().BeTrue();
    }
}