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
using Uttom.UnitTests.TestHelpers;

namespace Uttom.UnitTests.Handlers;

public class AddDelivererCommandHandlerTests : TestHelper, IDisposable, IAsyncDisposable
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly AddDelivererCommandHandler _handler;
    private readonly MotorcycleRepository _motorcycleRepository;
    private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    private readonly IDelivererRepository _delivererRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMinioService _minioService;
    private readonly IImageService _imageService;

    private const string PATH = "TestData/Images";

    public AddDelivererCommandHandlerTests()
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

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext,
            _motorcycleRepository,
            _registeredMotorCycleRepository,
            _delivererRepository,
            _rentalRepository);

        _handler = new AddDelivererCommandHandler(
            _uttomUnitOfWork,
            _minioService,
            _imageService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenBusinessTaxIdIsNotUnique()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "SEA",
            "Sara Elza Alves",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992,10,20),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            3,
            string.Empty);

        await _uttomUnitOfWork.DelivererRepository.AddAsync(Deliverer.Create(
            "MCF",
            "Mariah Catarina Figueiredo",
            command.BusinessTaxId,
            new DateTime(1950,5,2),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            DriverLicenseType.AB), CancellationToken.None);

        await _uttomUnitOfWork.SaveChangesAsync();

        await _handler.Handle(command, CancellationToken.None);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The business tax id must be unique.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenDriverLicenseNumberIsNotUnique()
    {
        // Arrange
        var command = new AddDelivererCommand(
            "SEA",
            "Sara Elza Alves",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992,10,20),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            3,
            string.Empty);

        await _uttomUnitOfWork.DelivererRepository.AddAsync(Deliverer.Create(
            "MCF",
            "Mariah Catarina Figueiredo",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1950,5,2),
            command.DriverLicenseNumber,
            DriverLicenseType.AB), CancellationToken.None);

        await _uttomUnitOfWork.SaveChangesAsync();

        await _handler.Handle(command, CancellationToken.None);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The driver license number must be unique.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenDelivererIsAdded()
    {
        // Arrange
         var base64ImageData = StringExtensions.ConvertToBase64($"{PATH}/cnh.png");

        var command = new AddDelivererCommand(
            "MM",
            "Matheus",
            GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992,10,20),
            GenerateDocument(DocumentType.DriverLicenseNumber),
            3,
            base64ImageData);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        // check if driver license image is stored in database
        var deliverer = await _uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId, CancellationToken.None);

        deliverer.DriverLicenseImageId.Should().NotBeNullOrEmpty();
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