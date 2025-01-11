namespace thirdpartynetwork.Core.Exceptions.HttpExceptions;

public class UnauthorizedHttpException : BaseHttpException
{
    public UnauthorizedHttpException()
        : base(401, "Client is unauthorized to access this resource.")
    {
    }
    
    public UnauthorizedHttpException(string message)
        : base(401, message)
    {
    }
}
