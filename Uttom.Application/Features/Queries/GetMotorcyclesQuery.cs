using MediatR;
using Uttom.Application.DTOs;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record GetMotorcyclesQuery(int PageNumber, int PageSize) : IRequest<ResultResponse<PagedResult<MotorcycleDto>>>;