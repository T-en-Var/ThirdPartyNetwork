using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Stores;

public static class AuthorizationCodeStore
{
    private static readonly List<AuthorizationCode> Codes = new();

    public static void Store(AuthorizationCode code)
    {
        Codes.Add(code);
    }

    public static AuthorizationCode? Get(string code)
    {
        return Codes.FirstOrDefault(x => x.Code == code && x.ExpiresAt > DateTime.UtcNow);
    }

    public static void Remove(string code)
    {
        Codes.RemoveAll(x => x.Code == code);
    }
}