using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.DTOs;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;

namespace Uttom.API.Controllers;

[ApiController]
[Route("/locacao")]
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

    [HttpGet("{id}/devolucao")]
    [SwaggerOperation(Summary = "Get rental price", Description = "Get rental price.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Total Rental Price", typeof(decimal))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error")]
    public async Task<IActionResult> GetRentalPrice([FromRoute] int id, [FromQuery] GetTotalRentalPriceQueryString query)
    {
        var result = await _mediator.Send(new GetTotalRentalPriceQuery(id, query.DataDevolucao));

        return !result.Success
            ? BadRequest(result.ErrorMessage)
            : Ok(result.Data);
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

    [HttpPut("{id}/devolucao")]
    [SwaggerOperation(Summary = "Add return date to a rental", Description = "Add return date to a rental.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Return date added successfully", typeof(string))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Rental not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input supplied")]
    public async Task<IActionResult> AddReturnDate([FromRoute] int id, [FromBody] UpdateRentalCommand command)
    {
        var updateRentalCommandWithRentalId = command.RentalId.HasValue
            ? command
            : command.WithRentalId(id);

        var result = await _mediator.Send(updateRentalCommandWithRentalId);

        return !result.Success
            ? BadRequest(new { message = result.ErrorMessage })
            : Ok(new { message = result.Data });
    }
}