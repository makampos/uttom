using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers;

public class UpdateMotorcycleCommandHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateMotorCycleCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public UpdateMotorcycleCommandHandlerTests()
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

        _handler = new UpdateMotorCycleCommandHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleIsNotFound()
    {
        // Arrange
        var command = new UpdateMotorcycleCommand(1, "DHA-1234");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenMotorcycleIsFound()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", "DHA-1234");

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var command = new UpdateMotorcycleCommand(entity.Id, "DHA-1111");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.PlateNumber.Should().Be("DHA-1111");

        // fetch the entity from the database

        var updatedEntity = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(entity.Id);

        updatedEntity.Should().NotBeNull();
        updatedEntity.PlateNumber.Should().Be("DHA-1111");

    }
}