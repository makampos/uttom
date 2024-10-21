using FluentAssertions;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class GetMotorcycleByPlateNumberQueryHandlerTests : BaseTestHandler<GetMotorCycleByPlateNumberQueryHandler>
{
    private readonly GetMotorCycleByPlateNumberQueryHandler _handler;

    public GetMotorcycleByPlateNumberQueryHandlerTests()
    {
        _handler = CreateHandler(_uttomUnitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenInvalidQueryIsGiven()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMotorcycleByPlateNumberQuery("DHA-1235"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Motorcycle not found.");
    }

    [Fact]
    public async Task Handle_ShouldReturnMotorcycle_WhenValidQueryIsGiven()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMotorcycleByPlateNumberQuery(entity.PlateNumber), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlateNumber.Should().Be(entity.PlateNumber);
    }
}