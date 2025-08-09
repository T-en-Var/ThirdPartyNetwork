using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Core.Helper
{
    public class CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine("[Middleware (Request)] " + context.Request.Path);
            try
            {
                await next(context);
            }
            catch (BaseHttpException ex)
            {
                logger.LogError($"BaseHttpException: {ex}");

                if (!context.Response.HasStarted)
                    await ErrorResponseWriter.WriteJsonError(context, ex.StatusCode, ex.ErrorMessage, ex.ErrorDetails);
            }
            catch (Exception ex)
            {
                logger.LogError($"Unhandled exception: {ex}");

                if (!context.Response.HasStarted)
                    await ErrorResponseWriter.WriteJsonError(context, 500, "An unexpected error occurred.", ex.Message);
            }
        }
    }
}
