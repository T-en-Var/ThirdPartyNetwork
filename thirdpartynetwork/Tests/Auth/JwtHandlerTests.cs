using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class JwtHandlerTests
{
    private const string Secret = "test-secret-which-is-long-enough";

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("TOKEN_SECRET", Secret);
    }

    [Test]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        var token = CreateToken("jti1", DateTime.UtcNow.AddMinutes(5));
        var handler = new JwtHandler();

        var result = handler.ValidateToken(token);

        Assert.That(result, Is.True);
    }

    [Test]
    public void ValidateToken_ExpiredToken_ReturnsFalse()
    {
        var token = CreateToken("jti2", DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddMinutes(-20));
        var handler = new JwtHandler();

        var result = handler.ValidateToken(token);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ValidateTokenPair_MatchingJti_ReturnsTrue()
    {
        var jti = Guid.NewGuid().ToString();
        var refresh = CreateToken(jti, DateTime.UtcNow.AddHours(1));

        var accessPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("jti", jti) }));

        var handler = new JwtHandler();

        var ok = handler.ValidateTokenPair(accessPrincipal, refresh);

        Assert.That(ok, Is.True);
    }

    [Test]
    public void ValidateTokenPair_NonMatchingJti_ReturnsFalse()
    {
        var refresh = CreateToken(Guid.NewGuid().ToString(), DateTime.UtcNow.AddHours(1));
        var accessPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("jti", "other") }));

        var handler = new JwtHandler();

        var ok = handler.ValidateTokenPair(accessPrincipal, refresh);

        Assert.That(ok, Is.False);
    }

    private static string CreateToken(string jti, DateTime expires, DateTime? notBefore = null)
    {
        var key = Encoding.UTF8.GetBytes(Secret);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("jti", jti) }),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            NotBefore = notBefore ?? DateTime.UtcNow,
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
