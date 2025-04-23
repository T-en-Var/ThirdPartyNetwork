using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Core.Helper
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var message = exception.Message;
            
            Console.WriteLine(exception.StackTrace);

            if (exception is BaseHttpException baseHttpException)
            {
                httpContext.Response.StatusCode = baseHttpException.StatusCode;
                message = baseHttpException.ErrorMessage;
            }

            return WriteResponseAsync(httpContext, httpContext.Response.StatusCode, message, exception.StackTrace);
        }

        private async ValueTask<bool> WriteResponseAsync(HttpContext context, int statusCode, string message, string? details = null)
        {
            try
            {
                await ErrorResponseWriter.WriteJsonError(context, statusCode, message, details);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Response writing was canceled.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while writing the response.");
                return false;
            }
        }
    }
}
