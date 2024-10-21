using FluentAssertions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;
using Uttom.Infrastructure.TestData;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddOrUpdateDriverLicenseCommandHandlerTests : BaseTestHandler<AddOrUpdateDriverLicenseCommandHandler>
{
    private readonly AddOrUpdateDriverLicenseCommandHandler _handler;

    public AddOrUpdateDriverLicenseCommandHandlerTests()
    {
        _handler = CreateHandler(parameters: new object[] { _uttomUnitOfWork, _minioService, _imageService, _logger });
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
}