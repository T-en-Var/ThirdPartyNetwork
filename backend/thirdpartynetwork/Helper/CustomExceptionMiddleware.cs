using System.Text.Json;
using thirdpartynetwork.Exceptions;

namespace thirdpartynetwork.Helper
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            System.Diagnostics.Debug.WriteLine("Hoi!");
            System.Diagnostics.Debug.WriteLine(context);
            try
            {
                await _next(context);
            }
            catch (BaseHttpException ex)
            {
                _logger.LogError($"CustomHttpException: {ex}");
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                string message = ex.ErrorDetails == null
                    ? JsonSerializer.Serialize(new
                    {
                        StatusCode = ex.StatusCode,
                        Message = ex.ErrorMessage
                    })
                    : JsonSerializer.Serialize(new
                    {
                        StatusCode = ex.StatusCode,
                        Message = ex.ErrorMessage,
                        Details = ex.ErrorDetails
                    });

                if (message == null)
                {
                    await context.Response.WriteAsync("Message could be loaded.");
                    throw new Exception("Message could be loaded.");
                }

                await context.Response.WriteAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception: {ex}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
