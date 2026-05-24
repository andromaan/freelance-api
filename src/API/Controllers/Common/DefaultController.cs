using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("")]
[ApiController]
public class DefaultController : ControllerBase
{
    [HttpGet]
    public ActionResult Index()
    {
        return Redirect("/swagger/index.html");
    }
}