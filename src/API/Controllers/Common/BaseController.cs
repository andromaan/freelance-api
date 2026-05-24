using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

public class BaseController : ControllerBase
{
    protected ActionResult GetResult<T>(Result<T> result)
    {
        return StatusCode((int)result.StatusCode, result.ToResponse());
    }
}