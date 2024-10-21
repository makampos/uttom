using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.DTOs;
using Uttom.Application.Features.Commands;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;

namespace Uttom.API.Controllers;

[ApiController]
[Route("api/motorcycles")]
[SwaggerTag("Motorcycles Service")]
public class MotorcycleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUttomUnitOfWork _unitOfWork;

    public MotorcycleController(IMediator mediator, IUttomUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [SwaggerOperation("Register a new motorcycle")]
    [SwaggerResponse(StatusCodes.Status201Created)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMotorcycle([FromBody] AddMotorcycleCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(null, null);
    }

    [HttpGet]
    [SwaggerOperation("Get all existing motorcycles")]
    [SwaggerResponse(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMotorcycles([FromQuery] GetMotorcyclesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("plate-number")]
    [SwaggerOperation("Get a motorcycle by plate number")]
    [SwaggerResponse(StatusCodes.Status200OK, "Motorcycle found", typeof(MotorcycleDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Motorcycle not found")]
    public async Task<IActionResult> GetMotorcycleByPlateNumber([FromQuery] GetMotorcycleByPlateNumberQuery query)
    {
        var result = await _mediator.Send(query);

        return !result.Success
            ? NotFound(result.ErrorMessage)
            : Ok(result.Data);
    }

    [HttpPut("{id}/plate-number")]
    [SwaggerOperation("Update a motorcycle's plate number")]
    [SwaggerResponse(StatusCodes.Status200OK, "Motorcycle updated", typeof(string))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Motorcycle not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation failed")]
    public async Task<IActionResult> UpdateMotorcyclePlateNumber([FromRoute] int id, [FromBody] UpdateMotorcycleCommand command)
    {
        var commandWithMotorcycleId = command.MotorcycleId.HasValue
            ? command
            : command.WithMotorcycleId(id);

        var result = await _mediator.Send(commandWithMotorcycleId);

        return !result.Success
            ? NotFound(result.ErrorMessage)
            : Ok(result.Data);
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Get a motorcycle by id")]
    [SwaggerResponse(StatusCodes.Status200OK, "Motorcycle found", typeof(Motorcycle))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Motorcycle not found")]
    public async Task<IActionResult> GetMotorcycleById(int id)
    {
        // Add DTO
        var result = await _unitOfWork.MotorcycleRepository.GetByIdAsync(id);

        return result is null
            ? NotFound("Motorcycle not found.")
            : Ok(result);
    }


    [HttpDelete("{id}")]
    [SwaggerOperation("Delete a motorcycle by id")]
    [SwaggerResponse(StatusCodes.Status200OK, "Motorcycle deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Motorcycle not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Motorcycle has rental record, or validation failed")]
    public async Task<IActionResult> DeleteMotorcycleById([FromRoute] DeleteMotorcycleCommand command)
    {
        var result = await _mediator.Send(command);

        return !result.Success
            ? NotFound("Motorcycle not found.")
            : Ok(result);
    }
}