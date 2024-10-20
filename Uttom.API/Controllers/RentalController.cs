using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;

namespace Uttom.API.Controllers;

[ApiController]
[Route("api/rentals")]
[SwaggerTag("Rental Service")]
public class RentalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public RentalController(IMediator mediator, IUttomUnitOfWork uttomUnitOfWork)
    {
        _mediator = mediator;
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    [HttpPost]
    [SwaggerOperation("Register a new rental")]
    [SwaggerResponse(StatusCodes.Status201Created)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRental([FromBody] AddRentalCommand command)
    {
        var result = await _mediator.Send(command);

        return !result.Success
            ? BadRequest(new { message = result.ErrorMessage })
            : CreatedAtAction(null, null);
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Get rental by id")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRentalById([FromRoute] int id)
    {
        //TODO: Add handler to get by id and return the specific contract for the endpoint, not db entity
        var result = await _uttomUnitOfWork.RentalRepository.GetByIdAsync(id);

        return result is null
            ? NotFound(new { message = "Rental not found." })
            : Ok(result);
    }
}