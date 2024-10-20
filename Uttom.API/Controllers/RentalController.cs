using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.DTOs;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;

namespace Uttom.API.Controllers;

[ApiController]
[Route("api/rentals")]
[SwaggerTag("Rental Service")]
public class RentalController : ControllerBase
{
    private readonly IMediator _mediator;

    public RentalController(IMediator mediator)
    {
        _mediator = mediator;
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
    [SwaggerOperation(Summary = "Retrieve a rental by its ID", Description = "Returns rental details for the specified ID.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Rental found", typeof(RentalDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Rental not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input supplied")]
    public async Task<IActionResult> GetRentalById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetRentalQuery(id));

        return !result.Success
            ? NotFound(new { message = "Rental not found." })
            : Ok(result.Data);
    }
}