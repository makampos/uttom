using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Uttom.Infrastructure.Implementations;

namespace Uttom.IntegrationTests.Fixtures;

public abstract class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly MinioContainer _minioContainer;

    protected CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("uttomdb")
            .WithUsername("uttom")
            .WithPassword("uttom77")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(5672)
            .WithPortBinding(15672)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        // Initialize Minio container
        _minioContainer = new MinioBuilder()
            .WithImage("minio/minio")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // should I get rabbit connection string before starting?
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        await _minioContainer.StartAsync();

        // var configuration = new ConfigurationBuilder()
        //     .AddInMemoryCollection(new Dictionary<string, string>
        //     {
        //         {"RabbitMq:Host", _rabbitMqContainer.Hostname},
        //         {"RabbitMq:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString()},
        //         {"RabbitMq:Username", "guest"},
        //         {"RabbitMq:Password", "guest"},
        //         {"Minio:Endpoint", $"{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}"},
        //         {"Minio:AccessKey", "minioadmin"},
        //         {"Minio:SecretKey", "minioadmin"}
        //     }!)
        //     .Build();

    }

    public string GetRabbitMqConnectionString()
        => $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}";

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

            //
        });
    }
}
