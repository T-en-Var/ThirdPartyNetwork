using System.Text.Json;

namespace thirdpartynetwork.Core.Helper;

public static class ErrorResponseWriter
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }
    }

    public static async Task WriteJsonError(HttpContext context, int statusCode, string message, string? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var error = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = details
        };

        var json = JsonSerializer.Serialize(error);
        await context.Response.WriteAsync(json);
    }
}