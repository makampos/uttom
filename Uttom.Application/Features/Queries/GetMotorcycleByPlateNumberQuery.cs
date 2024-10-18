using MediatR;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record GetMotorcycleByPlateNumberQuery(string PlateNumber) : IRequest<ResultResponse<Motorcycle>>;