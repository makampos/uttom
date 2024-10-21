using FluentAssertions;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.Handlers;

[Collection("Unit Tests")]
public class GetMotorcyclesQueryHandlerTests : BaseTestHandler<GetMotorcyclesQueryHandler>
{
    private readonly GetMotorcyclesQueryHandler _handler;

    public GetMotorcyclesQueryHandlerTests()
    {
        _handler = CreateHandler(_uttomUnitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entity = Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber());

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMultipleMotorcycles_WhenMotorcyclesAreFound()
    {
        // Arrange
        var entities = new List<Motorcycle>
        {
            Motorcycle.Create("Yamaha", 2020, "YZB", GeneratePlateNumber()),
            Motorcycle.Create("Honda", 2021, "HON", GeneratePlateNumber()),
            Motorcycle.Create("Suzuki", 2022, "SUZ", GeneratePlateNumber())
        };

        foreach (var entity in entities)
        {
            await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity);
        }

        await _uttomUnitOfWork.SaveChangesAsync();

        var request = new GetMotorcyclesQuery(1, 10);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().NotBeNullOrEmpty();
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