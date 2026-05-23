using System.Net;

namespace BLL.Services;

public class ServiceResponse<T>
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

    public static ServiceResponse<T> GetResponse(string message, bool success, T? data, HttpStatusCode statusCode)
    {
        return new ServiceResponse<T>
        {
            Message = message,
            Success = success,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ServiceResponse<T> Ok(string message = "Ok", T? data = default)
    {
        return GetResponse(message, true, data, HttpStatusCode.OK);
    }

    public static ServiceResponse<T> BadRequest(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.BadRequest);
    }

    public static ServiceResponse<T> InternalError(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.InternalServerError);
    }

    public static ServiceResponse<T> NotFound(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.NotFound);
    }

    public static ServiceResponse<T> Forbidden(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.Forbidden);
    }

    public static ServiceResponse<T> Unauthorized(string message, T? data = default)
    {
        return GetResponse(message, false, data, HttpStatusCode.Unauthorized);
    }
}