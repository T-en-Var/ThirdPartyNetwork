using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;

namespace thirdpartynetwork.Application.Auth;

// public class AdminRequirementHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
//     : AuthorizationHandler<AdminRequirement>
// {
//
//     protected override async Task HandleRequirementAsync(
//         AuthorizationHandlerContext context, AdminRequirement requirement)
//     {
//         var userClaims = httpContextAccessor.HttpContext?.User;
//
//         if (userClaims?.HasClaim(c => c.Type == requirement.ClaimType && c.Value == requirement.ClaimValue) == true)
//         {
//             var hasAccess = await userService.HasAdminAccessAsync(userClaims);
//             if (hasAccess)
//             {
//                 context.Succeed(requirement);
//             }
//         }
//     }
// }
