﻿using thirdpartynetwork.Application.Dtos.UserProxy;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IMeService : IBaseService
    {
        Task<UserProxy?> GetMeAsync();
        Task<ICollection<NetworkPermissionsCollectionDto>> GetMyPermissionsAsync();
    }
}
