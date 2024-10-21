using MassTransit;
using NSubstitute;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Interfaces.Services;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;
using Uttom.Infrastructure.Services;
using Uttom.UnitTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Uttom.UnitTests.Handlers;

public abstract class BaseTestHandler<THandler> : TestHelper, IDisposable, IAsyncDisposable where THandler : class
{
    protected readonly IUttomUnitOfWork _uttomUnitOfWork;
    protected readonly ApplicationDbContext _dbContext;
    protected readonly MotorcycleRepository _motorcycleRepository;
    protected readonly IRegisteredMotorCycleRepository _registeredMotorCycleRepository;
    protected readonly IDelivererRepository _delivererRepository;
    protected readonly IRentalRepository _rentalRepository;
    protected readonly IMinioService _minioService;
    protected readonly IImageService _imageService;
    protected readonly ILogger<THandler> _logger;
    protected readonly IBus _busControl = Substitute.For<IBus>();

    protected BaseTestHandler()
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
        _logger = new Logger<THandler>(new LoggerFactory());

        _uttomUnitOfWork = new UttomUnitOfWork(_dbContext,
            _motorcycleRepository,
            _registeredMotorCycleRepository,
            _delivererRepository,
            _rentalRepository);
    }

    protected THandler CreateHandler(params object[] parameters)
    {
        return (THandler)Activator.CreateInstance(typeof(THandler), parameters)!;
    }

    public virtual void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}