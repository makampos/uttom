using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;

namespace Uttom.IntegrationTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly MinioContainer _minioContainer;

    public CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("uttomdb")
            .WithUsername("uttom")
            .WithPassword("uttom77")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(5672)
            .WithPortBinding(15672)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _minioContainer = new MinioBuilder()
            .WithImage("minio/minio")
            .WithPortBinding(9000)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9000))
            .WithEnvironment("MINIO_ACCESS_KEY", "minioadmin")
            .WithEnvironment("MINIO_SECRET_KEY", "minioadmin")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        await _minioContainer.StartAsync();
    }

    protected override async void Dispose(bool disposing)
    {
        if (disposing)
        {
            await _postgresContainer.StopAsync();
            await _rabbitMqContainer.StopAsync();
            await _minioContainer.StopAsync();
        }
        base.Dispose(disposing);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }
        });
    }

    public async Task<Motorcycle?> GetMotorcycle(string plateNumber, CustomWebApplicationFactory _factory)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        return await uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(plateNumber, false);
    }

    public async Task<Deliverer?> GetDeliverer(string businessTaxId, CustomWebApplicationFactory _factory)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        return await uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(businessTaxId);
    }

    public async Task<Rental?> GetRentalByDelivererId(int delivererId, CustomWebApplicationFactory _factory)
    {
        using var scope = _factory.Services.CreateScope();
        var uttomUnitOfWork = scope.ServiceProvider.GetRequiredService<IUttomUnitOfWork>();
        return await uttomUnitOfWork.RentalRepository.GetByDelivererIdAsync(delivererId);
    }
}
