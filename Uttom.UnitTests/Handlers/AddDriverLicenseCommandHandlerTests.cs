using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Interfaces.Services;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;
using Uttom.Infrastructure.Services;

namespace Uttom.UnitTests.Handlers;

public class AddDriverLicenseCommandHandlerTests
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly AddDriverLicenseCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMinioService _minioService;
    private readonly IImageService _imageService;

    private const string PATH = "TestData/Images";

    public AddDriverLicenseCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _motorcycleRepository = new MotorcycleRepository(_dbContext);
        _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
        _delivererRepository = new DelivererRepository(_dbContext);
        _rentalRepository = new RentalRepository(_dbContext);
        _minioService = new MinioService();
        _imageService = new ImageService();

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext, _motorcycleRepository, _registeredMotorCycleRepository, _delivererRepository, _rentalRepository);

        _handler = new AddDriverLicenseCommandHandler(_uttomUnitOfWork, _minioService, _imageService);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenDelivererIsNotFound()
    {
        // Arrange
        var command = new AddDriverLicenseCommand("base64string", 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Deliverer not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenImageExtensionIsNotValid()
    {
        // Arrange
        var entity = Deliverer.Create(
            "SEA",
            "Sara Elza Alves",
            "20.681.653/0001-90",
            new DateTime(1992, 10, 20),
            "59375336842", DriverLicenseType.AB);

        await _uttomUnitOfWork.DelivererRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.jpeg");

        // Act
        var result = await _handler.Handle(new AddDriverLicenseCommand(base64ImageData, entity.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The image extension is not valid.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenDelivererIsFound()
    {
       // Arrange
       var entity = Deliverer.Create(
           "SEA",
           "Sara Elza Alves",
           "20.681.653/0001-90",
           new DateTime(1992, 10, 20),
           "59375336842", DriverLicenseType.AB);

       await _uttomUnitOfWork.DelivererRepository.AddAsync(entity);
       await _uttomUnitOfWork.SaveChangesAsync();

       var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.png");

       var entityWithoutImage = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(entity.Id);
       entityWithoutImage!.DriverLicenseImageId.Should().BeNullOrEmpty();

       // Act
       var result = await _handler.Handle(new AddDriverLicenseCommand(base64ImageData, entity.Id ), CancellationToken.None);

       // Assert
       result.Should().NotBeNull();
       result.Success.Should().BeTrue();
       entity.DriverLicenseImageId.Should().NotBeNullOrEmpty();
    }
}