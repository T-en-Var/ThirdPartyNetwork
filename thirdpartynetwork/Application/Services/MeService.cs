﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class MeService(IUserProxyService userProxyService, IIdentityService identityService) : IMeService
    {
        public async Task<UserProxy?> GetMeAsync()
        {
            var appUser = identityService.GetClaimsPrincipal();
            if (appUser == null) throw new UnauthorizedHttpException("No HttpContext User");

            var uidClaim = appUser.Claims.FirstOrDefault(x => x.Type == "uid");
            if (uidClaim == null) throw new UnauthorizedHttpException("Invalid token 1 (Me)");

            var userProxyId = Guid.Parse(uidClaim.Value);
            
            var userProxy = await userProxyService.GetByIdAsync(userProxyId);
            return userProxy;
        }
    }
}
