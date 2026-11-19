namespace thirdpartynetwork.Core.Exceptions.HttpExceptions;

public class UnauthorizedHttpException : BaseHttpException
{
    public static string BaseMessage = "Client is unauthorized to access this resource.";
    
    public UnauthorizedHttpException()
        : base(403, BaseMessage)
    {
    }
    
    public UnauthorizedHttpException(string message)
        : base(403, message)
    {
    }
    
    public UnauthorizedHttpException(string message, string details)
        : base(403, message, details)
    {
    }
}
