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
            httpContext.Response.ContentType = "application/json";

            var message = exception.Message;
            
            Console.WriteLine(exception.StackTrace);

            if (exception is BaseHttpException baseHttpException)
            {
                httpContext.Response.StatusCode = baseHttpException.StatusCode;
                message = baseHttpException.ErrorMessage;
            }

            var response = new
            {
                message,
            };

            var responseBody = JsonSerializer.Serialize(response);
            return WriteResponseAsync(httpContext, responseBody, cancellationToken);
        }

        private async ValueTask<bool> WriteResponseAsync(HttpContext httpContext, string responseBody, CancellationToken cancellationToken)
        {
            try
            {
                await httpContext.Response.WriteAsync(responseBody, cancellationToken);
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
