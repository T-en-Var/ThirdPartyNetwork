namespace thirdpartynetwork.Exceptions
{
    public class NotFoundHttpException : BaseHttpException
    {
        public NotFoundHttpException()
            : base(404, "The requested resource was not found.")
        {
        }

        public NotFoundHttpException(string message)
            : base(404, message)
        {
        }

        public NotFoundHttpException(string message, string details)
            : base(404, message, details)
        {
        }

        public NotFoundHttpException(string message, Exception innerException)
            : base(404, message, innerException)
        {
        }
    }
}
