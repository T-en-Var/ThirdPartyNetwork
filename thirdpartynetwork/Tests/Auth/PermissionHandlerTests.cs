using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Tests.Auth
{
    [TestFixture]
    public class PermissionHandlerTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IRoleVerifier> _roleVerifierMock;
        private Mock<IIdentityVerifier> _identityVerifierMock;
        private PermissionHandler _permissionHandler;
        private AuthorizationHandlerContext _authContext;
        private ClaimsPrincipal _user;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _roleVerifierMock = new Mock<IRoleVerifier>();
            _identityVerifierMock = new Mock<IIdentityVerifier>();

            _permissionHandler = new PermissionHandler(
                _httpContextAccessorMock.Object,
                _roleVerifierMock.Object,
                _identityVerifierMock.Object
            );

            var claims = new List<Claim>
            {
                new Claim("uid", Guid.NewGuid().ToString())
            };

            var identity = new ClaimsIdentity(claims);
            _user = new ClaimsPrincipal(identity);

            _authContext = new AuthorizationHandlerContext(
                new[] { new PermissionRequirement(Constants.Permissions.EditNetwork) },
                _user,
                null
            );
        }

        [Test]
        public async Task HandleRequirementAsync_UserHasPermission_Succeeds()
        {
            // Arrange
            _roleVerifierMock.Setup(rv =>
                    rv.IsSuperAdminOrHasSuperPermission(It.IsAny<Guid>(), Constants.Permissions.EditNetwork))
                .Returns(true);

            // Act
            await _permissionHandler.HandleAsync(_authContext);

            // Assert
            Assert.That(_authContext.HasSucceeded, Is.True);
        }

        [Test]
        public async Task HandleRequirementAsync_UserLacksPermission_Fails()
        {
            // Arrange
            _roleVerifierMock.Setup(rv =>
                    rv.IsSuperAdminOrHasSuperPermission(It.IsAny<Guid>(), Constants.Permissions.EditNetwork))
                .Returns(false);

            // Act
            await _permissionHandler.HandleAsync(_authContext);

            // Assert
            Assert.That(_authContext.HasSucceeded, Is.False);
        }
    }
}