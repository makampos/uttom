using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddRentalCommand(int PlanId, int DeliverId, int MotorcycleId, DateOnly StartDate, DateOnly EstimatingEndingDate) : IRequest<ResultResponse<bool>>;