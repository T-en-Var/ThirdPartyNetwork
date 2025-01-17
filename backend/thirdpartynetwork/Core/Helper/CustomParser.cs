namespace thirdpartynetwork.Core.Helper;

public static class CustomParser
{
    public static Guid GetGuidFromHttpContextUrl(HttpContext httpContext, string guidName)
    {
        object? guidValue = null;
        var guid = Guid.Empty;
        
        var hasNetworkId = httpContext?.Request.RouteValues.TryGetValue(guidName, out guidValue) == true;
        var isValidGuid = guidValue is string networkIdString && Guid.TryParse(networkIdString, out guid);
        
        if (!hasNetworkId || !isValidGuid) return Guid.Empty;
        return guid == Guid.Empty ? Guid.Empty : guid;
    }
}
