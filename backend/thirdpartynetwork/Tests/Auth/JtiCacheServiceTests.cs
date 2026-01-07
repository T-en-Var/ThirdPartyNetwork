using System;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class JtiCacheServiceTests
{
    [Test]
    public void StoreJti_And_IsJtiValid_Works()
    {
        var svc = new JtiCacheService();
        svc.StoreJti("a", null, TimeSpan.FromMinutes(10));
        svc.StoreJti("b", "a", TimeSpan.FromMinutes(10));
        
        Assert.That(svc.IsJtiValid("a"), Is.True);
        Assert.That(svc.IsJtiValid("b"), Is.True);
    }

    [Test]
    public void AppendValidateJti_InvalidPrevious_Throws()
    {
        var svc = new JtiCacheService();

        var ex = Assert.Throws<UnauthorizedHttpException>(() =>
            svc.StoreJti("new", "missing", TimeSpan.FromMinutes(10)));

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void RemoveJti_RemovesTreeAndGarbageCollects()
    {
        var svc = new JtiCacheService();
        svc.StoreJti("root", null, TimeSpan.FromSeconds(1));
        svc.StoreJti("child", "root", TimeSpan.FromMinutes(1));

        Assert.That(svc.IsJtiValid("root"), Is.True);
        Assert.That(svc.IsJtiValid("child"), Is.True);

        svc.RemoveJti("root");

        Assert.That(svc.IsJtiValid("root"), Is.False);
        Assert.That(svc.IsJtiValid("child"), Is.False);
    }
}
