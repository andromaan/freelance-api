using System.Net;

namespace BLL.Services;

public class Result<T>
{
    public required string Message { get; set; }
    public bool Success { get; set; }
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public object ToResponse() => new
    {
        Message,
        Success,
        Data
    };

    public static Result<T> GetResponse(string message, bool success, T? data, HttpStatusCode statusCode)
    {
        return new Result<T>
        {
            Message = message,
            Success = success,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static Result<T> Ok(string message = "Ok", T? data = default)
    {
        return GetResponse(message, true, data, HttpStatusCode.OK);
    }

    public static Result<T> BadRequest(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.BadRequest);
    }

    public static Result<T> InternalError(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.InternalServerError);
    }

    public static Result<T> NotFound(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.NotFound);
    }

    public static Result<T> Forbidden(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.Forbidden);
    }

    public static Result<T> Unauthorized(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.Unauthorized);
    }
}