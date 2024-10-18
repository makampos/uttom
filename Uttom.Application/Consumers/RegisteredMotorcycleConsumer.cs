using MassTransit;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Messages;

namespace Uttom.Application.Consumers;

public class RegisteredMotorcycleConsumer : IConsumer<RegisteredMotorcycle>
{
    private readonly IUttomUnitOfWork _unitOfWork;

    public RegisteredMotorcycleConsumer(IUttomUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<RegisteredMotorcycle> context)
    {
        var message = context.Message;
        await _unitOfWork.RegisteredMotorCyclesRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();
    }
}