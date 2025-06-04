using System.Security.Cryptography;
using System.Text;

namespace thirdpartynetwork.Application.Auth;

public class PKCEVerifier
{
    public static bool VerifyPKCE(string codeVerifier, string codeChallenge)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hash = sha256.ComputeHash(bytes);
        var computedChallenge = Base64UrlEncode(hash);
        return computedChallenge == codeChallenge;
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}