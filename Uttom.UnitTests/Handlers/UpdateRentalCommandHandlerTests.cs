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
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class UpdateRentalCommandHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateRentalCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;

    public UpdateRentalCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase-"+Guid.NewGuid())
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

        _handler = new UpdateRentalCommandHandler(_uttomUnitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturn_FailureResult_WhenRentalIsNotFound()
    {
        // Arrange
        var command = new UpdateRentalCommand(new DateOnly(2023, 10, 20), 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Should().NotBeNull();
        result.ErrorMessage.Should().Be("Rental not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturn_FailureResult_WhenReturnDateIsLessThanRentalStartDate()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);

        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var endDate = command.StartDate.AddDays(RentalPlans.GetPlan(7)!.Days);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var rentalEntity = Rental.Create(7,
            endDate,
            command.EstimatingEndingDate,
            existingDeliverer.Id,
            existingMotorcycle.Id);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rentalEntity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var updateRentalCommand = new UpdateRentalCommand(DateOnly.FromDateTime(DateTime.Today).AddDays(-1), rentalEntity.Id);

        // Act
        var result = await _handler.Handle(updateRentalCommand, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Return date cannot be before the rental start date.");
    }

    [Fact]
    public async Task Handle_ShouldReturn_SuccessResult_WhenReturnDateIsGreaterThanRentalStartDate()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);

        var command = new AddRentalCommand(7,
            existingDeliverer.Id,
            existingMotorcycle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10));

        var endDate = command.StartDate.AddDays(RentalPlans.GetPlan(7)!.Days);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var rentalEntity = Rental.Create(7,
            endDate,
            command.EstimatingEndingDate,
            existingDeliverer.Id,
            existingMotorcycle.Id);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rentalEntity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var updateRentalCommand = new UpdateRentalCommand(endDate, rentalEntity.Id);

        // Act
        var result = await _handler.Handle(updateRentalCommand, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
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