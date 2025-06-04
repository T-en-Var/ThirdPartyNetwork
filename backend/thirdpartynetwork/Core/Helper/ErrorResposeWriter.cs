using System.Text.Json;

namespace thirdpartynetwork.Core.Helper;

public static class ErrorResponseWriter
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public required string Message { get; set; }
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
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(error, options);
        // string stackTrace = Environment.StackTrace;

        // Console.WriteLine(stackTrace);
        Console.WriteLine(json);
        await context.Response.WriteAsync(json);
    }
}