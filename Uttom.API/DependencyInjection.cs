using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Infrastructure.Implementations;
using Uttom.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Uttom.Application.Consumers;
using Uttom.Application.Features.Handlers;
using Uttom.Application.Validators;
using Uttom.Domain.Interfaces.Services;
using Uttom.Infrastructure.Services;

namespace Uttom.API;

public static class DependencyInjection
{
    public static IServiceCollection BaseRegister(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterRepositories()
            .RegisterDBContext(configuration)
            .RegisterMediatR()
            .RegisterBus(configuration)
            .RegisterServices()
            .RegisterSwagger()
            .RegisterValidators();

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUttomUnitOfWork), typeof(UttomUnitOfWork));
        services.AddScoped(typeof(IMotorcycleRepository), typeof(MotorcycleRepository));
        services.AddScoped(typeof(IRegisteredMotorCycleRepository), typeof(RegisteredMotorCycleRepository));
        services.AddScoped(typeof(IDelivererRepository), typeof(DelivererRepository));
        services.AddScoped(typeof(IRentalRepository), typeof(RentalRepository));
        return services;
    }

    private static IServiceCollection RegisterDBContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<DbContext, ApplicationDbContext>();

        return services;
    }

    public static void MigrateDatabase(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while migrating the database.", ex);
            }
        }
    }

    private static IServiceCollection RegisterMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly, typeof(AddMotorCycleCommandHandler).Assembly);
        });

        return services;
    }

    private static IServiceCollection RegisterBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<RegisteredMotorcycleConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitMqConnectionString!),  h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("motorcycles_queue", e =>
                {
                    e.ConfigureConsumer<RegisteredMotorcycleConsumer>(context);
                });
            });
        });

        return services;
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IMinioService, MinioService>();
        services.AddScoped<IImageService, ImageService>();
        return services;
    }

    private static IServiceCollection RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Uttom Swagger",
                Contact = new OpenApiContact
                {
                    Name = "Uttom Development Team",
                    Email = "Uttom@uttom.fake",
                    Url = new Uri("https://uttom.fake")
                }
            });
        });

        return services;
    }

    private static IServiceCollection RegisterValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AddDelivererCommandValidator>();
        services.AddFluentValidationAutoValidation();
        return services;
    }
}