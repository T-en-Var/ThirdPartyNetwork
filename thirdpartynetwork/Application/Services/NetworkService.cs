﻿using AutoMapper;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(
        INetworkRepository repository,
        IPermissionService permissionService,
        IMapper mapper,
        IIdentityService identityService,
        ICDNService cdnService,
        IIdentityVerifier identityVerifier,
        IFileRepository fileRepository // im sorry
        ) : INetworkService
    {
        private static Network? _cachedMainNetwork;

        private async Task<ICollection<Network>> FilterNetworksAsync(ICollection<Network> networks, bool bypass = false)
        {
            if (bypass) return networks;

            var currentUser = await identityService.GetUserProxyAsync(true);
            var isSuper =
                await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(currentUser.Id, Constants.Permissions.ReadNetwork);

            var allowedNetworks = new List<Network>();

            foreach (var network in networks)
            {
                if (network.IsPublic 
                    || isSuper 
                    || (await identityVerifier.IsAllowedAsync(network.Id, currentUser.Id, Constants.Permissions.ReadNetwork)))
                {
                    allowedNetworks.Add(network);
                }
            }

            return allowedNetworks;
        }

        public async Task<ICollection<Network>> GetAllAsync()
        {
            var networks = await repository.GetAllAsync();
            return await FilterNetworksAsync(networks);
        }

        public async Task<Network?> GetByIdAsync(Guid id, bool bypass = false)
        {
            var network = await repository.GetByIdAsync(id, bypass);
            if (network == null) return null;

            var networks = await FilterNetworksAsync([network], bypass);
            return networks.Count > 0 ? networks.First() : null;
        }

        public async Task<Network?> GetDetailsByIdAsync(Guid id)
        {
            var network = await repository.GetDetailsByIdAsync(id);
            if (network == null) return null;

            var currentUser = await identityService.GetUserProxyAsync(omitThrow: true);

            network.NetworkUsers = network.NetworkUsers
                .Where(x => x.UserProxyId == currentUser.Id).ToList();

            return network;
        }

        public async Task<Network> CreateAsync(CreateNetworkDto createNetworkDto)
        {
            var nameFound = await repository.GetByNameAsync(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            var network = mapper.Map<Network>(createNetworkDto);

            if (createNetworkDto.FileLink != null)
            {
                network.ImageFileId = createNetworkDto.FileLink.Id;
            }

            var userRole = new Role
            {
                Name = "User",
                Description = "Default role for new users.",
                IsDefault = true,
                Network = network
            };

            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Default admin role.",
                IsDefault = false,
                Network = network
            };

            adminRole.RolePermissions =
            [
                new RolePermission
                {
                    Role = adminRole,
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.Administrator))!
                }
            ];

            userRole.RolePermissions = new List<RolePermission>()
            {
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadNetwork))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadPermission))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadAccess))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadFile))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadRole))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadCustomPage))!,
                    Role = userRole
                },
                new()
                {
                    Permission = (await permissionService.GetByNameAsync(Constants.Permissions.ReadPageBlock))!,
                    Role = userRole
                },
            };

            var currentUser = await identityService.GetUserProxyAsync();
            network.Roles = [userRole, adminRole];

            network.NetworkUsers =
            [
                new NetworkUser()
                {
                    NetworkId = network.Id,
                    UserProxyId = currentUser.Id,
                    Network = network,
                    UserProxy = currentUser,
                    NetworkUserRoles =
                    [
                        new NetworkUserRole
                        {
                            NetworkUser = null,
                            Role = adminRole
                        }
                    ]
                }
            ];
            network = await repository.CreateAsync(network);

            return network;
        }

        public async Task<Network> UpdateAsync(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            if (!await repository.ExistsAsync(id)) throw new NotFoundHttpException();
            
            var network = mapper.Map<Network>(updateNetworkDto);
            network.Id = id;

            if (updateNetworkDto.FileLink != null)
            {
                network.ImageFileId = updateNetworkDto.FileLink.Id;
            }

            network = await repository.UpdateAsync(network);
            return network;
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var network = await GetByIdAsync(id, true);
            if (network == null) return;

            network.ImageFileId = null;
            network.ImageFile = null;

            var files = await fileRepository.GetByNetworkIdAsync(network.Id);

            foreach (var networkFile in files)
            {
                await cdnService.DeleteAsync($"{networkFile.NetworkId}/{networkFile.PublicId}", networkFile.MediaType);
            }

            await repository.DeleteAsync(network);
        }

        public async Task<Network> GetMainNetworkAsync()
        {
            if (_cachedMainNetwork != null)
                return _cachedMainNetwork;

            _cachedMainNetwork = await repository.GetMainNetworkAsync();
            return _cachedMainNetwork;
        }

        public IQueryable<Network> GetMainNetworkQueryable()
        {
            var mainNetwork = repository.GetByNameQueryable(Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");

            return mainNetwork;
        }

        public IQueryable<Network> GetByIdQueryable(Guid id)
        {
            var network = repository.GetByIdQueryable(id);
            return network;
        }
    }
}