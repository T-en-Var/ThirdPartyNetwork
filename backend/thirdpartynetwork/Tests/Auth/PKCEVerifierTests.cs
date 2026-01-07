using NUnit.Framework;
using thirdpartynetwork.Application.Auth;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class PKCEVerifierTests
{
    [Test]
    public void VerifyPKCE_ValidVerifier_ReturnsTrue()
    {
        // A known verifier/challenge pair
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        var challenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        var result = PKCEVerifier.VerifyPKCE(verifier, challenge);

        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyPKCE_InvalidVerifier_ReturnsFalse()
    {
        var verifier = "invalid-verifier";
        var challenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        var result = PKCEVerifier.VerifyPKCE(verifier, challenge);

        Assert.That(result, Is.False);
    }
}
