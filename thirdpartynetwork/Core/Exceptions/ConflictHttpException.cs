namespace thirdpartynetwork.Core.Exceptions
{
    public class ConflictHttpException : BaseHttpException
    {
        public ConflictHttpException()
            : base(409, "Detected conflict in resources.")
        {
        }

        public ConflictHttpException(string message)
            : base(409, message)
        {
        }

        public ConflictHttpException(string message, string details)
            : base(409, message, details)
        {
        }

        public ConflictHttpException(string message, Exception innerException)
            : base(409, message, innerException)
        {
        }
    }
}
