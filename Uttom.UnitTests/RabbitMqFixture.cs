using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Uttom.UnitTests;

public class RabbitMqFixture : IAsyncDisposable
{
    private readonly IContainer _rabbitMqContainer = new ContainerBuilder()
        .WithImage("rabbitmq:3-management")
        .WithPortBinding(5672)
        .WithPortBinding(15672)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
        .Build();

    public async Task StartAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public string GetRabbitMqConnectionString()
    {
        return $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}";
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}