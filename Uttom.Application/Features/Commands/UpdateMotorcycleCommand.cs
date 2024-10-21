using MediatR;
using Uttom.Application.DTOs;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record UpdateMotorcycleCommand(string PlateNumber, int? MotorcycleId = null) : IRequest<ResultResponse<string>>
{
    public UpdateMotorcycleCommand WithMotorcycleId(int motorcycleId) => this with { MotorcycleId = motorcycleId };
}