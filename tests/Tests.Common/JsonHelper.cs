using System.Net.Http.Json;
using System.Text.Json;
using BLL.Services;

namespace Tests.Common;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T> GetPayloadAsync<T>(HttpResponseMessage response)
    {
        var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<T>>(DefaultOptions)
                              ?? throw new InvalidOperationException("Response content is null or invalid.");

        var dataJson = JsonSerializer.Serialize(serviceResponse.Data, DefaultOptions);

        var data = JsonSerializer.Deserialize<T>(dataJson, DefaultOptions)
                      ?? throw new InvalidOperationException("Failed to deserialize data.");

        return data;
    }
}
