using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddDelivererCommand(
    string Identifier,
    string Name,
    string BusinessTaxId,
    DateTime DateOfBirth,
    string DriverLicenseNumber,
    int DriverLicenseType,
    string? DriverLicenseImageBase64String) : IRequest<ResultResponse<bool>>;