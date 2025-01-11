namespace thirdpartynetwork.Core.Exceptions.HttpExceptions
{
    public class UnprocessableEntityHttpException : BaseHttpException
    {
        public UnprocessableEntityHttpException()
            : base(422, "Provided data was incomplete.")
        {
        }

        public UnprocessableEntityHttpException(string message)
            : base(422, message)
        {
        }

        public UnprocessableEntityHttpException(string message, string details)
            : base(422, message, details)
        {
        }

        public UnprocessableEntityHttpException(string message, Exception innerException)
            : base(422, message, innerException)
        {
        }
    }
}
