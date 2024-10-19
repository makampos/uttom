using DotNet.Testcontainers.Builders;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Uttom.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly MinioContainer _minioContainer;

    public IntegrationTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("uttomdb")
            .WithUsername("uttom")
            .WithPassword("uttom77")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672)
            .WithPortBinding(15672)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _minioContainer = new MinioBuilder()
            .WithImage("minio/minio") // Specify the image explicitly
            // .WithRootUser("minioadmin")
            // .WithRootPassword("minioadmin")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
        await _minioContainer.StartAsync();
    }

    public string GetPostgresConnectionString() =>
        _postgresContainer.GetConnectionString();

    public string GetRabbitMqConnectionString()
        => $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}";

    public string GetMinioEndpoint() =>
        $"{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}";

    public async ValueTask DisposeAsync()
    {
        await _minioContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
        await _postgresContainer.StopAsync();
    }
}