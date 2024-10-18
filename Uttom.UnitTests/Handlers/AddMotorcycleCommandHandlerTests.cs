using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Handlers;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;

namespace Uttom.UnitTests.Handlers
{
    public class AddMotorcycleCommandHandlerTests : IClassFixture<RabbitMqFixture>
    {
        private readonly IUttomUnitOfWork _uttomUnitOfWork;
        private readonly ApplicationDbContext _dbContext;
        private readonly AddMotorCycleCommandHandler _handler;
        private readonly MotorcycleRepository _motorcycleRepository;
        private readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
        private readonly IDelivererRepository _delivererRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly RabbitMqFixture _rabbitMqFixture;

        private readonly IBusControl _busControl;

        public AddMotorcycleCommandHandlerTests(RabbitMqFixture rabbitMqFixture)
        {
            _rabbitMqFixture = rabbitMqFixture;

            _rabbitMqFixture.StartAsync().GetAwaiter().GetResult();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _motorcycleRepository = new MotorcycleRepository(_dbContext);
            _registeredMotorCycleRepository = new RegisteredMotorCycleRepository(_dbContext);
            _delivererRepository = new DelivererRepository(_dbContext);
            _rentalRepository = new RentalRepository(_dbContext);

            _uttomUnitOfWork = new UttomUnitOfWork(_dbContext, _motorcycleRepository, _registeredMotorCycleRepository, _delivererRepository, _rentalRepository);

             _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var rabbitMqConnectionString = _rabbitMqFixture.GetRabbitMqConnectionString();
                cfg.Host(new Uri(rabbitMqConnectionString), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });

            _handler = new AddMotorCycleCommandHandler(_uttomUnitOfWork, _busControl);
        }

        [Fact]
        public async Task Handle_ShouldAddMotorcycle_WhenValidCommandIsGiven()
        {
            // Arrange
            var command = new AddMotorcycleCommand("Yamaha", 2020, "YZB", "155000");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(command.PlateNumber, false);
            motorcycle.Should().NotBeNull();
            motorcycle.Identifier.Should().Be("Yamaha");
            motorcycle.Year.Should().Be(2020);
            motorcycle.Model.Should().Be("YZB");
            motorcycle.PlateNumber.Should().Be("155000");
        }

        [Fact]
        public async Task Handle_ShouldNotAddMotorcycle_WhenPlateNumberAlreadyExists()
        {
            // Arrange
            var command = new AddMotorcycleCommand("Yamaha", 2020, "YZB", "155000");

            var existingMotorcycle = Motorcycle.Create("Yamaha", 2020, "YZB", "155000");
            await _dbContext.Motorcycles.AddAsync(existingMotorcycle);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("The plate number must be unique.");
        }
    }
}