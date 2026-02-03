﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserService(
        INetworkUserRepository repository,
        INetworkService networkService,
        IUserProxyService userProxyService,
        IMapper mapper) : INetworkUserService
    {
        public async Task<ICollection<NetworkUser>> GetAllAsync()
        {
            var networkUsers = await repository.GetAllAsync();
            return networkUsers;
        }

        public async Task<NetworkUser?> GetByIdsAsync(Guid networkId, Guid userProxyId)
        {
            var networkUser = await repository.GetByIdsAsync(networkId, userProxyId);
            return networkUser;
        }

        public async Task<NetworkUser?> GetByIdsPreAuthAsync(Guid networkId, Guid userProxyId)
        {
            var networkUser = await repository.GetByIdsPreAuthAsync(networkId, userProxyId);
            return networkUser;
        }

        public async Task<ICollection<NetworkUser>> GetByNetworkIdAsync(Guid networkId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not Found");

            var networkUsers = await repository.GetByNetworkIdAsync(networkId);
            return networkUsers;
        }

        public async Task<ICollection<NetworkUser>> GetByUserProxyIdAsync(Guid userProxyId)
        {
            if (await userProxyService.GetByIdAsync(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");

            var networkUsers = await repository.GetByUserProxyIdAsync(userProxyId);
            return networkUsers;
        }

        public async Task<ICollection<Network>> GetNetworksByUserProxyIdAsync(Guid userProxyId)
        {
            if (await userProxyService.GetByIdAsync(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");
            
            var networkUsers = await repository.GetByUserProxyIdAsync(userProxyId, q => q
                    .Include(n => n.Network)
                    .ThenInclude(n => n.ImageFile));
            
            var networks = networkUsers.Select(nu => nu.Network).ToList();
            return networks;
        }

        public async Task<ICollection<File>> GetFilesByUserProxyIdAsync(Guid userProxyId)
        {
            if (await userProxyService.GetByIdAsync(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");

            var networkUsers = await repository.GetByUserProxyIdAsync(userProxyId, q => q
                .Include(nu => nu.Files));
            
            var files = networkUsers
                .SelectMany(nu => nu.Files)
                .ToList();

            return files;
        }

        public async Task<NetworkUser?> GetByIdAsync(Guid networkId, Guid id)
        {
            var networkUser = await repository.GetByIdAsync(networkId, id);
            return networkUser;
        } 
        
        public async Task<NetworkUser?> GetJustByIdAsync(Guid id)
        {
            return await repository.GetJustByIdAsync(id);
        }

        public async Task<NetworkUser?> GetByNetworkIdAndEmailAsync(Guid networkId, string email)
        {
            var networkUser = await repository.GetByNetworkIdAndEmailAsync(networkId, email);
            return networkUser;
        }

        public async Task<NetworkUser> CreateAsync(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser)
        {
            var network = await networkService.GetByIdAsync(networkId, bypass: true);
            if (network == null) throw new NotFoundHttpException("Network not Found");

            var userProxy = await userProxyService.GetByIdAsync(userProxyId);
            if (userProxy == null) throw new NotFoundHttpException("UserProxy not Found");

            var isNotDefaultFlag = userProxy.Email == null;
            userProxy = userProxy.ApplyDefaults();

            if (await GetByIdsAsync(networkId, userProxyId) != null) throw new ConflictHttpException("Email is already in use.");
            if (await GetByNetworkIdAndEmailAsync(networkId, userProxy.Email) != null)
                throw new ConflictHttpException("User with this email already exists" +
                                                (isNotDefaultFlag
                                                    ? ". (You email was imported from your default proxy)"
                                                    : ""));

            var networkUser = mapper.Map<NetworkUser>(createNetworkUser);
            networkUser.NetworkId = networkId;
            networkUser.UserProxyId = userProxyId;

            foreach (var networkAccess in network.NetworkAccesses)
            {
                networkUser.NetworkUserAccesses.Add(new NetworkUserAccess()
                {
                    AccessId = networkAccess.AccessId,
                    IsAccepted = networkAccess.IsRequired,
                    NetworkUserId = networkUser.Id,
                    Access = default!,
                    NetworkUser = default!,
                });
            }

            foreach (var role in network.Roles)
            {
                if (!role.IsDefault) continue;

                networkUser.NetworkUserRoles.Add(new NetworkUserRole()
                {
                    RoleId = role.Id,
                    NetworkUserId = networkUser.Id,
                    Role = default!,
                    NetworkUser = default!,
                });
            }

            networkUser = await repository.CreateAsync(networkUser);
            return networkUser;
        }

        public async Task<NetworkUser> UpdateAsync(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not Found");
            if (!await repository.ExistsAsync(networkUserId)) throw new NotFoundHttpException();

            var networkUser = mapper.Map<NetworkUser>(updateNetworkUser);
            networkUser.Id = networkUserId;
            networkUser.NetworkId = networkId;

            networkUser = await repository.UpdateAsync(networkId, networkUser);
            return networkUser;
        }

        public async Task NetworkAccessUpdateAsync(Guid networkId)
        {
            await repository.NetworkAccessUpdateAsync(networkId);
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid id)
        {
            var network = await networkService.GetByIdAsync(networkId);
            if (network is { IsSystemProtected: true })
                throw new UnauthorizedHttpException("Network is system protected");

            var networkUser = await repository.GetByIdAsync(networkId, id);
            if (networkUser != null) await repository.DeleteAsync(networkUser);
        }
    }
}