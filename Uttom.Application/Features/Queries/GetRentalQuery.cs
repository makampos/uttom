using MediatR;
using Uttom.Application.DTOs;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record GetRentalQuery(int RentalId) : IRequest<ResultResponse<RentalDto>>;