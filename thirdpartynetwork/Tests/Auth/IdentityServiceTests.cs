using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class IdentityServiceTests
{
    private DbContextOptions<DataContext> _options;

    [SetUp]
    public void SetUp()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public void GetUserProxy_ReturnsCached_WhenPresent()
    {
        using var context = new DataContext(_options);
        var user = new User();
        var proxy = new UserProxy { Id = Guid.NewGuid(), IsDefault = true, User = user };
        // user.DefaultProxy = proxy;
        context.UserProxies.Add(proxy);
        context.SaveChanges();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("uid", proxy.Id.ToString()) }));

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var svc = new IdentityService(accessorMock.Object, context);

        var p1 = svc.GetUserProxy();
        var p2 = svc.GetUserProxy();

        Assert.That(p1.Id, Is.EqualTo(p2.Id));
    }

    [Test]
    public void GetUserProxy_OmitThrow_ReturnsDefault_WhenMissing()
    {
        using var context = new DataContext(_options);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var svc = new IdentityService(accessorMock.Object, context);

        var proxy = svc.GetUserProxy(true);

        Assert.That(proxy, Is.Not.Null);
        Assert.That(proxy.User, Is.Null);
    }

    [Test]
    public void IsAuthenticated_FalseWhenNoUidClaim()
    {
        using var context = new DataContext(_options);
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var svc = new IdentityService(accessorMock.Object, context);

        Assert.That(svc.IsAuthenticated(), Is.False);
    }
}
