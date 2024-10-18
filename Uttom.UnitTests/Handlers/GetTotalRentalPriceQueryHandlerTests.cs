using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers;

public class GetTotalRentalPriceQueryHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTotalRentalPriceQueryHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public GetTotalRentalPriceQueryHandlerTests()
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

        _handler = new GetTotalRentalPriceQueryHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturn_TotalPrice_WhenRentalIsFound()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", "20.681.653/0001-9",
            new DateTime(1992, 10, 20), "59375336842", DriverLicenseType.A);

        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var endDate = command.StartDate.AddDays(RentalPlans.GetPlan(7)!.Days);

        var rentalEntity = Rental.Create(7,
            endDate,
            command.EstimatingEndingDate,
            existingDeliverer.Id,
            existingMotorcycle.Id);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.RentalRepository.AddAsync(rentalEntity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var actualReturnDate = endDate.AddDays(-2);
        var query = new CalculateTotalRentalPriceQuery(rentalEntity.Id, actualReturnDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(162m);
    }
}