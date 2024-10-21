using FluentAssertions;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class UpdateMotorcycleCommandHandlerTests : BaseTestHandler<UpdateMotorCycleCommandHandler>
{
    private readonly UpdateMotorCycleCommandHandler _handler;

    public UpdateMotorcycleCommandHandlerTests()
    {
        _handler = CreateHandler(_uttomUnitOfWork, _logger);
    }
    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMotorcycleIsNotFound()
    {
        // Arrange
        var command = new UpdateMotorcycleCommand(GeneratePlateNumber(), 100);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenMotorcycleIsFound()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var command = new UpdateMotorcycleCommand(entity.PlateNumber, entity.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be("Motorcycle updated successfully.");

        var updatedEntity = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(entity.Id);

        updatedEntity.Should().NotBeNull();
        updatedEntity.PlateNumber.Should().Be(entity.PlateNumber);

    }
}