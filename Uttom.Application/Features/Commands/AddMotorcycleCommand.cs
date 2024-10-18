using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddMotorcycleCommand(string Identifier, int Year, string Model, string PlateNumber) : IRequest<ResultResponse<bool>>;