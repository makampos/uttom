using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
using Uttom.Infrastructure.TestData;
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddOrUpdateDriverLicenseCommandHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly AddOrUpdateDriverLicenseCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMinioService _minioService;
    private readonly IImageService _imageService;
    private readonly ILogger<AddOrUpdateDriverLicenseCommandHandler> _logger;

    public AddOrUpdateDriverLicenseCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase"+Guid.NewGuid())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _motorcycleRepository = new MotorcycleRepository(_dbContext);
        _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
        _delivererRepository = new DelivererRepository(_dbContext);
        _rentalRepository = new RentalRepository(_dbContext);
        _minioService = new MinioService();
        _imageService = new ImageService();
        _logger = new Logger<AddOrUpdateDriverLicenseCommandHandler>(new LoggerFactory());

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext, _motorcycleRepository, _registeredMotorCycleRepository, _delivererRepository, _rentalRepository);

        _handler = new AddOrUpdateDriverLicenseCommandHandler(_uttomUnitOfWork, _minioService, _imageService, _logger);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenDelivererIsNotFound()
    {
        // Arrange
        var command = new AddOrUpdateDriverLicenseCommand(string.Empty, 1);

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
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20),
            GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.AB);

        await _uttomUnitOfWork.DelivererRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var base64ImageData = ImageConverter.ConvertToBase64("cnh.jpeg");

        // Act
        var result = await _handler.Handle(new AddOrUpdateDriverLicenseCommand(base64ImageData, entity.Id), CancellationToken.None);

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
           GenerateDocument(DocumentType.BusinessTaxId),
           new DateTime(1992, 10, 20),
           GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.AB);

       await _uttomUnitOfWork.DelivererRepository.AddAsync(entity);
       await _uttomUnitOfWork.SaveChangesAsync();

       var base64ImageData = ImageConverter.ConvertToBase64("cnh.png");

       var entityWithoutImage = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(entity.Id);
       entityWithoutImage!.DriverLicenseImageId.Should().BeNullOrEmpty();

       // Act
       var result = await _handler.Handle(new AddOrUpdateDriverLicenseCommand(base64ImageData, entity.Id ), CancellationToken.None);

       // Assert
       result.Should().NotBeNull();
       result.Success.Should().BeTrue();
       entity.DriverLicenseImageId.Should().NotBeNullOrEmpty();
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