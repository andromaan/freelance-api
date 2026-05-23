using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

public class BaseController : ControllerBase
{
    protected ActionResult GetResult<T>(ServiceResponse<T> serviceResponse)
    {
        return StatusCode((int)serviceResponse.StatusCode, serviceResponse.ToResponse());
    }
}