using FluentAssertions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;
using Uttom.Infrastructure.TestData;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddDelivererCommandHandlerTests : BaseTestHandler<AddDelivererCommandHandler>
{
    private readonly AddDelivererCommandHandler _handler;
    public AddDelivererCommandHandlerTests()
    {
        _handler = CreateHandler(parameters: new object[]{ _uttomUnitOfWork, _minioService, _imageService, _logger} );
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
         var base64ImageData = ImageConverter.ConvertToBase64("cnh.png");

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

        var deliverer = await _uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId, CancellationToken.None);

        deliverer.DriverLicenseImageId.Should().NotBeNullOrEmpty();
    }
}