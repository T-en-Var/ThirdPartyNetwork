namespace thirdpartynetwork.Exceptions
{
    public class BaseHttpException : Exception
    {
        public int StatusCode { get; set; } = 500;
        public string ErrorMessage { get; set; } = "An error has occurred";

        public string? ErrorDetails { get; set; }

        public BaseHttpException() { }

        public BaseHttpException(int statusCode)
        {
            StatusCode = statusCode;
        }

        public BaseHttpException(int statusCode, string message) : this(statusCode)
        {
            ErrorMessage = message;
        }

        public BaseHttpException(int statusCode, string message, string details) : this(statusCode, message)
        {
            ErrorDetails = details;
        }

        public BaseHttpException(int statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorMessage = message;
        }

        public override string ToString()
        {
            return $"HttpException: {StatusCode} - {ErrorMessage} | Details: {ErrorDetails ?? "None"}";
        }
    }
}
