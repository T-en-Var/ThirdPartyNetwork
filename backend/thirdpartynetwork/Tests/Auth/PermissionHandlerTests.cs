// using System.Security.Claims;
// using Microsoft.AspNetCore.Authorization;
// using Moq;
// using NUnit.Framework;
// using thirdpartynetwork.Application.Auth;
// using thirdpartynetwork.Application.Auth.Requirements;
// using thirdpartynetwork.Core;
// using thirdpartynetwork.Core.Interfaces.Auth;
//
// namespace thirdpartynetwork.Tests.Auth
// {
//     [TestFixture]
//     public class PermissionHandlerTests
//     {
//         private Mock<IHttpContextAccessor> _httpContextAccessorMock;
//         private Mock<IUserProxyService> _userProxyServiceMock;
//         private Mock<IIdentityVerifier> _identityVerifierMock;
//         private Mock<IIdentityService> _identityServiceMock;
//         private PermissionHandler _permissionHandler;
//         private AuthorizationHandlerContext _authContext;
//         private ClaimsPrincipal _user;
//
//         [SetUp]
//         public void Setup()
//         {
//             _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
//             _userProxyServiceMock = new Mock<IUserProxyService>();
//             _identityVerifierMock = new Mock<IIdentityVerifier>();
//             _identityServiceMock = new Mock<IIdentityService>();
//
//             _permissionHandler = new PermissionHandler(
//                 _httpContextAccessorMock.Object,
//                 _userProxyServiceMock.Object,
//                 _identityVerifierMock.Object,
//                 _identityServiceMock.Object,
//             );
//
//             var claims = new List<Claim>
//             {
//                 new Claim("uid", Guid.NewGuid().ToString())
//             };
//
//             var identity = new ClaimsIdentity(claims);
//             _user = new ClaimsPrincipal(identity);
//
//             _authContext = new AuthorizationHandlerContext(
//                 new[] { new PermissionRequirement(Constants.Permissions.ManageNetwork) },
//                 _user,
//                 null
//             );
//         }
//         #pragma warning restore CS8618
//
//         [Test]
//         public async Task HandleRequirementAsync_UserHasPermission_Succeeds()
//         {
//             // Arrange
//             _identityVerifierMock.Setup(rv =>
//                     rv.IsSuperAdminOrHasSuperPermission(It.IsAny<Guid>(), Constants.Permissions.ManageNetwork))
//                 .Returns(true);
//
//             // Act
//             await _permissionHandler.HandleAsync(_authContext);
//
//             // Assert
//             Assert.That(_authContext.HasSucceeded, Is.True);
//         }
//
//         [Test]
//         public async Task HandleRequirementAsync_UserLacksPermission_Fails()
//         {
//             // Arrange
//             _identityVerifierMock.Setup(rv =>
//                     rv.IsSuperAdminOrHasSuperPermission(It.IsAny<Guid>(), Constants.Permissions.ManageNetwork))
//                 .Returns(false);
//
//             // Act
//             await _permissionHandler.HandleAsync(_authContext);
//
//             // Assert
//             Assert.That(_authContext.HasSucceeded, Is.False);
//         }
//     }
// }