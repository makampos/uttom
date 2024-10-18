using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers;

public class GetMotorcyclesQueryHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetMotorCyclesQueryHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetMotorcyclesQueryHandlerTests()
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

        _handler = new GetMotorCyclesQueryHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenNoMotorcyclesAreFound()
    {
        // Arrange
        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycles not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", "DHA-1234");

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entities = new List<Motorcycle>
        {
            Motorcycle.Create("Yamaha", 2020, "YZB", "DHA-1234"),
            Motorcycle.Create("Honda", 2021, "HON", "DHA-1235"),
            Motorcycle.Create("Suzuki", 2022, "SUZ", "DHA-1236")
        };

        foreach (var entity in entities)
        {
            await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        }

        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalCount.Should().Be(3);
        result.Data.Items.Should().HaveCount(3);
    }
}