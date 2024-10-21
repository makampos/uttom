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
public class GetMotorcyclesQueryHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetMotorcyclesQueryHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetMotorcyclesQueryHandlerTests()
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

        _handler = new GetMotorcyclesQueryHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entities = new List<Motorcycle>
        {
            Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber()),
            Motorcycle.Create("Honda", 2021, "HON", GeneratePlateNumber()),
            Motorcycle.Create("Suzuki", 2022, "SUZ", GeneratePlateNumber())
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
        result.Data.Items.Should().NotBeNullOrEmpty();
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