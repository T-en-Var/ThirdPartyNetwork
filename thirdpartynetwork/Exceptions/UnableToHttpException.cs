namespace thirdpartynetwork.Exceptions
{
    public class UnableToHttpException : BaseHttpException
    {
        public UnableToHttpException()
            : base(500, "Unable to complete process.")
        {
        }

        public UnableToHttpException(string topic)
            : base(404, "Unable to " + topic + ".")
        {
        }

        public UnableToHttpException(string message, string details)
            : base(404, message, details)
        {
        }

        public UnableToHttpException(string message, Exception innerException)
            : base(404, message, innerException)
        {
        }
    }
}
