namespace thirdpartynetwork.Core.Helper;

public static class CustomParser
{
    public static Guid GetGuidFromHttpContextUrl(HttpContext httpContext, string guidName)
    {
        object? guidValue = null;
        var guid = Guid.Empty;
        
        var hasId = httpContext?.Request.RouteValues.TryGetValue(guidName, out guidValue) == true;
        var isValidGuid = guidValue is string idString && Guid.TryParse(idString, out guid);
        
        if (!hasId || !isValidGuid) return Guid.Empty;
        return guid == Guid.Empty ? Guid.Empty : guid;
    }
    
    public static Guid GetGuidFromHttpContextUserClaim(HttpContext httpContext, string guidName)
    {
        var appUser = httpContext.User;
        
        var userIdClaim = appUser.FindFirst(guidName);
        if (userIdClaim == null 
            || !Guid.TryParse(userIdClaim.Value, out var guid) 
            || guid == Guid.Empty) 
            return Guid.Empty;

        return guid;
    }
}
