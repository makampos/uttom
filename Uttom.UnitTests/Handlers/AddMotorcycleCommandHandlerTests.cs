using FluentAssertions;
using NSubstitute;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Messages;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class AddMotorcycleCommandHandlerTests : BaseTestHandler<AddMotorCycleCommandHandler>
{
    private readonly AddMotorCycleCommandHandler _handler;
    public AddMotorcycleCommandHandlerTests()
    {
        _handler = CreateHandler(parameters: new object[]{ _uttomUnitOfWork, _busControl, _logger });
    }

    [Fact]
    public async Task Handle_ShouldAddMotorcycle_WhenValidCommandIsGiven()
    {
        // Arrange
        _busControl.Publish(Arg.Any<RegisteredMotorcycle>()).Returns(Task.CompletedTask);
        var command = new AddMotorcycleCommand("Yamaha", 2024, "YZB", GeneratePlateNumber());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(command.PlateNumber, false);
        motorcycle.Should().NotBeNull();
        motorcycle.Identifier.Should().Be("Yamaha");
        motorcycle.Year.Should().Be(2024);
        motorcycle.Model.Should().Be("YZB");
        motorcycle.PlateNumber.Should().Be(command.PlateNumber);
    }

    [Fact]
    public async Task Handle_ShouldNotAddMotorcycle_WhenPlateNumberAlreadyExists()
    {
        // Arrange
        var command = new AddMotorcycleCommand("Yamaha", 2020, "YZB", GeneratePlateNumber());

        var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", command.PlateNumber);
        await _dbContext.Motorcycles.AddAsync(existingMotorcycle);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("The plate number must be unique.");
    }
}