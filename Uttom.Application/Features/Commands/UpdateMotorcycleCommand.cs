using MediatR;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record UpdateMotorcycleCommand(int Id, string PlateNumber) : IRequest<ResultResponse<Motorcycle>>;