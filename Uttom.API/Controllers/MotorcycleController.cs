using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Results;

namespace Uttom.API.Controllers;


[ApiController]
[Route("api/motorcycles")]
[SwaggerTag("Motorcycles Service")]
public class MotorcycleController : ControllerBase
{
    private readonly IMediator _mediator;

    public MotorcycleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [SwaggerOperation("Description")]
    [SwaggerResponse(StatusCodes.Status200OK, "Added", typeof(int))]
    public async Task<ActionResult<ResultResponse<bool>>> CreateMotorcycle([FromBody] AddMotorcycleCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return  BadRequest(result);
        }

        return Ok(result);
    }
}