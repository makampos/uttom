using FluentAssertions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Enum;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class DeleteMotorcycleCommandHandlerTests : BaseTestHandler<DeleteMotorCycleCommandHandler>
{
   private readonly DeleteMotorCycleCommandHandler _handler;

   public DeleteMotorcycleCommandHandlerTests()
   {
       _handler = CreateHandler(parameters: new object[] { _uttomUnitOfWork, _logger });
   }

   [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleIsNotFound()
    {
        // Arrange
        var request = new DeleteMotorcycleCommand(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleHasRentalRecord()
    {
        // Arrange
        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());
        var existingDeliverer = Deliverer.Create("SEA", "Sara Elza Alves", GenerateDocument(DocumentType.BusinessTaxId),
            new DateTime(1992, 10, 20), GenerateDocument(DocumentType.DriverLicenseNumber), DriverLicenseType.A);
        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(existingMotorcycle);
        await _uttomUnitOfWork.DelivererRepository.AddAsync(existingDeliverer);
        await _uttomUnitOfWork.SaveChangesAsync();

        var rental = Rental.Create(7, DateOnly.FromDateTime(DateTime.Today).AddDays(7),
            DateOnly.FromDateTime(DateTime.Today).AddDays(10), existingDeliverer.Id, existingMotorcycle.Id);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rental);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new DeleteMotorcycleCommand(existingMotorcycle.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle has rental record.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenMotorcycleIsDeleted()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new DeleteMotorcycleCommand(entity.Id);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        var deletedEntity = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(entity.PlateNumber, true);

        deletedEntity.Should().NotBeNull();
        deletedEntity.IsDeleted.Should().BeTrue();
    }
}