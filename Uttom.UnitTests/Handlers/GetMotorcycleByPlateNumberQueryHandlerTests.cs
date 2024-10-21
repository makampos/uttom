using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class GetMotorcycleByPlateNumberQueryHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetMotorCycleByPlateNumberQueryHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetMotorcycleByPlateNumberQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase"+Guid.NewGuid())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _motorcycleRepository = new MotorcycleRepository(_dbContext);
        _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
        _delivererRepository = new DelivererRepository(_dbContext);
        _rentalRepository = new RentalRepository(_dbContext);

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext, _motorcycleRepository, _registeredMotorCycleRepository, _delivererRepository, _rentalRepository);

        _handler = new GetMotorCycleByPlateNumberQueryHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenInvalidQueryIsGiven()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMotorcycleByPlateNumberQuery("DHA-1235"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnMotorcycle_WhenValidQueryIsGiven()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMotorcycleByPlateNumberQuery(entity.PlateNumber), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlateNumber.Should().Be(entity.PlateNumber);
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