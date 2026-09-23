using Microsoft.AspNetCore.Mvc;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses;

namespace PassKee.Api.Controllers;

[ApiController]
[Route(ApiUrl.Ping)]
public class PingController : ControllerBase
{
    [HttpGet]
    public ActionResult<PingResponse> Get()
    {
        return Ok(new PingResponse { Message = "Pong" });
    }
}
