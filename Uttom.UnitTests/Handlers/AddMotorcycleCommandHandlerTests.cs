using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Messages;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddMotorcycleCommandHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly AddMotorCycleCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    private readonly IBusControl _busControl;

    public AddMotorcycleCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _motorcycleRepository = new MotorcycleRepository(_dbContext);
        _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
        _delivererRepository = new DelivererRepository(_dbContext);
        _rentalRepository = new RentalRepository(_dbContext);

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext,
            _motorcycleRepository,
            _registeredMotorCycleRepository,
            _delivererRepository,
            _rentalRepository);

        _busControl = Substitute.For<IBusControl>();

        _handler = new AddMotorCycleCommandHandler(_uttomUnitOfWork, _busControl);
    }

    [Fact]
    public async Task Handle_ShouldAddMotorcycle_WhenValidCommandIsGiven()
    {
        // Arrange
        _busControl.Publish(Arg.Any<RegisteredMotorcycle>()).Returns(Task.CompletedTask);
        var command = new AddMotorcycleCommand("Yamaha", 2024, "YZB", GeneratePlateNumber());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(command.PlateNumber, false);
        motorcycle.Should().NotBeNull();
        motorcycle.Identifier.Should().Be("Yamaha");
        motorcycle.Year.Should().Be(2024);
        motorcycle.Model.Should().Be("YZB");
        motorcycle.PlateNumber.Should().Be(command.PlateNumber);
    }

    [Fact]
    public async Task Handle_ShouldNotAddMotorcycle_WhenPlateNumberAlreadyExists()
    {
        // Arrange
        var command = new AddMotorcycleCommand("Yamaha", 2020, "YZB", GeneratePlateNumber());

        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", command.PlateNumber);
        await _dbContext.Motorcycles.AddAsync(existingMotorcycle);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The plate number must be unique.");
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