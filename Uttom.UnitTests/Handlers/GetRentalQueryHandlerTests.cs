using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers;

public class GetRentalQueryHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetRentalQueryHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetRentalQueryHandlerTests()
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

        _handler = new GetRentalQueryHandler(_uttomUnitOfWork);
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
        var motorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "DHA-1234");
        var deliverer = Deliverer.Create(
            "SEA",
            "Sara Elza Alves",
            "20.681.653/0001-90",
            new DateTime(1992, 10, 20),
            "59375336842", DriverLicenseType.AB);

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
}