using System.Net;
using BLL.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Middlewares;

public class MiddlewareExceptionsHandling(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (SecurityTokenException ex)
        {
            await context.Response.WriteJsonResponseAsync(StatusCodes.Status426UpgradeRequired,
                Result<object>.GetResponse(ex.Message, false, null, HttpStatusCode.UpgradeRequired));
        }
        catch (ValidationException ex)
        {
            await context.Response.WriteJsonResponseAsync(StatusCodes.Status400BadRequest,
                Result<object>.BadRequest(ex.Message ?? throw new ArgumentNullException(nameof(ex))));
        }
        catch (Exception ex)
        {
            await context.Response.WriteJsonResponseAsync(StatusCodes.Status500InternalServerError,
                Result<object>.InternalError(ex.Message));
        }
    }
}