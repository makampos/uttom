using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.Features.Commands;

namespace Uttom.API.Controllers;

[ApiController]
[Route("/entregadores")]
[SwaggerTag("Deliverer Service")]
public class DelivererController : ControllerBase
{
    private readonly IMediator _mediator;

    public DelivererController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [SwaggerOperation("Register a new deliverer")]
    [SwaggerResponse(StatusCodes.Status201Created)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDeliverer([FromBody] AddDelivererCommand command)
    {
        var result = await _mediator.Send(command);

        return !result.Success
            ? BadRequest(new { message = result.ErrorMessage })
            : CreatedAtAction(null, null);
    }

    [HttpPost("{id}/cnh")]
    [SwaggerOperation("Upload driver license image")]
    [SwaggerResponse(StatusCodes.Status201Created)]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDriverLicenseImage([FromRoute] int id, [FromBody] AddOrUpdateDriverLicenseCommand command)
    {
        var result = await _mediator.Send(command.AddDelivererId(id));

        return !result.Success
            ? BadRequest(new { message = result.ErrorMessage })
            : CreatedAtAction(null, null);
    }
}