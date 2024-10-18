using System.Text.Json;
using MassTransit;
using MediatR;
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

    public AddMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork, IBus bus)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _bus = bus;
    }

    public async Task<ResultResponse<bool>> Handle(AddMotorcycleCommand command, CancellationToken cancellationToken)
    {
        // Check if plate number already exists
        var motorCycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(command.PlateNumber, false, cancellationToken);
        if (motorCycle is not null)
        {
            return ResultResponse<bool>.FailureResult("The plate number must be unique.");
        }

        var entity = Motorcycle.Create(command.Identifier, command.Year, command.Model, command.PlateNumber);

        await _uttomUnitOfWork.MotorcycleRepository.AddAsync(entity, cancellationToken);
        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

       var message = RegisteredMotorcycle.Create(entity.Identifier, entity.Year, entity.Model, entity.PlateNumber);

        // Outbox Pattern might be a good approach to handle inconsistency

        if (command.Year == 2024)
        {
            try
            {
                await _bus.Publish(message, cancellationToken);
                return ResultResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Error publishing message to RabbitMQ.", ex);
            }
        }

        return ResultResponse<bool>.SuccessResult(true);

    }
}