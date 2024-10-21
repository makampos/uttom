using System.Text.Json;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Messages;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class AddMotorCycleCommandHandler : IRequestHandler<AddMotorcycleCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly IBus _bus;
    private readonly ILogger<AddMotorCycleCommandHandler> _logger;

    public AddMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork, IBus bus, ILogger<AddMotorCycleCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _bus = bus;
        _logger = logger;
    }

    public async Task<ResultResponse<bool>> Handle(AddMotorcycleCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var motorCycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(command.PlateNumber, false, cancellationToken);
            if (motorCycle is not null)
            {
                _logger.LogWarning("Attempted to add motorcycle with duplicate plate number: {PlateNumber}", command.PlateNumber);
                return ResultResponse<bool>.FailureResult("The plate number must be unique.");
            }

            var entity = Motorcycle.Create(command.Identifier, command.Year, command.Model, command.PlateNumber);
            await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            var message = RegisteredMotorcycle.Create(entity.Identifier, entity.Year, entity.Model, entity.PlateNumber);

            if (command.Year == 2024)
            {
                try
                {
                    await _bus.Publish(message, cancellationToken);
                    _logger.LogInformation("Published message for registered motorcycle: {Message}", JsonSerializer.Serialize(message));
                    return ResultResponse<bool>.SuccessResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing message to RabbitMQ for motorcycle: {PlateNumber}", command.PlateNumber);
                    throw new Exception("Error publishing message to RabbitMQ.", ex);
                }
            }

            return ResultResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a motorcycle: {Message}", ex.Message);
            return ResultResponse<bool>.FailureResult("An unexpected error occurred.");
        }
    }
}