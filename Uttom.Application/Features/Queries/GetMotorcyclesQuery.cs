using MediatR;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record GetMotorcyclesQuery(int PageNumber, int PageSize) : IRequest<ResultResponse<PagedResult<Motorcycle>>>;