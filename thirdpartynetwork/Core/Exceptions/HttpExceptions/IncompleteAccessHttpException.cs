namespace thirdpartynetwork.Core.Exceptions.HttpExceptions;

public class IncompleteAccessHttpException : BaseHttpException
{
    public static string BaseMessage = "User has incomplete access and needs to be updated.";
    
    public IncompleteAccessHttpException()
        : base(499, BaseMessage)
    {
    }
    
    public IncompleteAccessHttpException(string message)
        : base(499, message)
    {
    }
    
    public IncompleteAccessHttpException(string message, string details)
        : base(499, message, details)
    {
    }
}
