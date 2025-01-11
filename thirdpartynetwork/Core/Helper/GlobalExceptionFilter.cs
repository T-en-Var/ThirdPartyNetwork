using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Web.Http;

namespace thirdpartynetwork.Core.Helper
{
    public class GlobalExceptionFilter(ILogger<CustomExceptionMiddleware> logger) : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            System.Diagnostics.Debug.WriteLine("Here Exception!");

            if (context.Exception is HttpResponseException httpResponseException)
            {
                // Extract details from HttpResponseException
                const int statusCode = 400; // (int)httpResponseException.StatusCode
                var errorMessage = httpResponseException.Message;

                context.Result = new ObjectResult(new { Error = errorMessage })
                {
                    StatusCode = statusCode
                };
            }
            else
            {
                // Handle all other exceptions with a generic error message
                logger.LogError(context.Exception.Message);
                logger.LogTrace(context.Exception.StackTrace);
                context.Result = new ObjectResult(new { Error = $"An unexpected error occurred. {context}, {context.Exception.Message}" })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            context.ExceptionHandled = true; // Mark the exception as handled
        }
    }
}
