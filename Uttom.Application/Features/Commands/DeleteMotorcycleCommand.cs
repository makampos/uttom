using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record DeleteMotorcycleCommand(int Id) : IRequest<ResultResponse<bool>>;