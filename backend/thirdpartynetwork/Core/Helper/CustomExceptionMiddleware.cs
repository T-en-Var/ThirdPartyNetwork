using System.Text.Json;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Core.Helper
{
    public class CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BaseHttpException ex)
            {
                logger.LogError($"CustomHttpException: {ex}");
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
                logger.LogError($"Unhandled exception: {ex}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Unhandled exception: {ex}");
                // await context.Response.WriteAsync($"An unexpected error occurred.");
            }
        }
    }
}
