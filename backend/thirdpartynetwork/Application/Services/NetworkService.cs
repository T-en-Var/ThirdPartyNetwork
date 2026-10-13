using System.Text.Json;
using AutoMapper;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkService(
        INetworkRepository repository,
        INetworkEntitlementRepository entitlementRepository,
        IPermissionService permissionService,
        IMapper mapper,
        IIdentityService identityService,
        ICDNService cdnService,
        IIdentityVerifier identityVerifier,
        IAccessLevelVerifier accessLevelVerifier,
        IDynamicAudienceCache audienceCache,
        IFileRepository fileRepository, // im sorry
        IPublicImageService publicImageService
    ) : INetworkService
    {
        private static Network? _cachedMainNetwork;

        private async Task<ICollection<Network>> FilterNetworksAsync(ICollection<Network> networks, bool bypass = false)
        {
            if (bypass) return networks;

            var currentUser = await identityService.GetUserProxyAsync(true);
            var isSuper =
                await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(currentUser.Id,
                    Constants.Permissions.ReadNetwork);

            var allowedNetworks = new List<Network>();

            foreach (var network in networks)
            {
                if (network.IsPublic
                    || isSuper
                    || (await identityVerifier.IsAllowedAsync(network.Id, currentUser.Id,
                        Constants.Permissions.ReadNetwork)))
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

        public async Task<ICollection<NetworkMetricsDto>> GetAllWithMetricsAsync()
        {
            return await repository.GetWithMetricsAsync();
        }

        public async Task<NetworkMetricsDto> GetByIdWithMetricsAsync(Guid networkId)
        {
            var metrics = (await repository.GetWithMetricsAsync(networkId)).First();
            if (metrics == null) throw new NotFoundHttpException("Network not found");
            return metrics;
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
            if (createNetworkDto.Name == Constants.Networks.Main)
                throw new ConflictHttpException("Cannot name network as main network.");
            
            var nameFound = await repository.GetByNameAsync(createNetworkDto.Name);
            if (nameFound != null)
                throw new ConflictHttpException("Duplicate Network name.");

            var network = mapper.Map<Network>(createNetworkDto);

            if (createNetworkDto.FileLink != null)
            {
                var imageFile = await fileRepository.GetByIdBypassingAccessAsync(createNetworkDto.FileLink.Id);
                if (imageFile == null) throw new NotFoundHttpException("Image file not found");

                if (!await accessLevelVerifier.CanNetworkExternallyAdoptAsync(imageFile, ContentKind.File, Guid.Empty))
                    throw new UnauthorizedHttpException("That file's network does not allow its files to be reused externally.");

                network.ImageFileId = createNetworkDto.FileLink.Id;
                await publicImageService.MakePubliclyViewableAsync(createNetworkDto.FileLink.Id);
            }
            
            // TODO:
            // These roles and their permissions should move to a file with readonly values
            // or be configurable.
            // The entitlements too.

            var userRole = new Role
            {
                Name = "User",
                Description = "Default role for new users.",
                IsDefault = true,
                Network = network,
                Entitlements = JsonDocument.Parse("{}")
            };

            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Default admin role.",
                IsDefault = false,
                Network = network,
                Entitlements = JsonDocument.Parse("{}")
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
                    // UserProxy = currentUser,
                    NetworkUserRoles =
                    [
                        new NetworkUserRole
                        {
                            NetworkUser = null,
                            Role = adminRole
                        }
                    ],
                    Entitlements = JsonDocument.Parse("{}")
                }
            ];

            network.Entitlement = new NetworkEntitlement()
            {
                NetworkId = network.Id,

                AllowBlogs = false,
                BlogCountLimit = 0,

                AllowConfigurations = false,
                ConfigurationCountLimit = 0,

                AllowFiles = false,
                FileCountLimit = 0,
                FileSizeLimit = 0,
                FileStorageLimit = 0,

                AllowCustomPages = false,
                CustomPageCountLimit = 0,
                CustomPageBlockCountLimit = 0,
                CustomPageBlockSizeLimit = 0,
            };

            network = await repository.CreateAsync(network);

            await RefreshIssuerCacheAsync();

            return network;
        }

        public async Task<Network> UpdateAsync(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            if (!await repository.ExistsAsync(id)) throw new NotFoundHttpException();

            if (updateNetworkDto.ExternalContentKinds is { } externalContentKinds &&
                !FlagsEnumValidator.IsDefinedCombination(externalContentKinds))
            {
                throw new UnprocessableEntityHttpException("ExternalContentKinds contains an unknown flag.");
            }

            var network = mapper.Map<Network>(updateNetworkDto);
            network.Id = id;

            if (updateNetworkDto.FileLink != null)
            {
                var imageFile = await fileRepository.GetByIdBypassingAccessAsync(updateNetworkDto.FileLink.Id);
                if (imageFile == null) throw new NotFoundHttpException("Image file not found");

                if (!await accessLevelVerifier.CanNetworkExternallyAdoptAsync(imageFile, ContentKind.File, id))
                    throw new UnauthorizedHttpException("That file's network does not allow its files to be reused externally.");

                network.ImageFileId = updateNetworkDto.FileLink.Id;
                await publicImageService.MakePubliclyViewableAsync(updateNetworkDto.FileLink.Id);
            }

            await RefreshIssuerCacheAsync();

            network = await repository.UpdateAsync(network);
            return network;
        }

        public async Task<NetworkEntitlement> UpdateEntitlementAsync(Guid networkId,
            UpdateNetworkEntitlementDto updateNetworkEntitlementDto)
        {
            if (!await entitlementRepository.ExistsAsync(networkId))
                throw new NotFoundHttpException();

            var mainNetwork = await GetMainNetworkDetailsAsync();

            if (networkId == mainNetwork.Id)
            {
                var mainEntitlement = mapper.Map<NetworkEntitlement>(updateNetworkEntitlementDto);
                mainEntitlement.NetworkId = networkId;
                return await entitlementRepository.UpdateAsync(mainEntitlement);
            }

            var allEntitlements = await GetAllAsync();

            ValidateGlobalLimits(networkId, updateNetworkEntitlementDto, mainNetwork.Entitlement,
                allEntitlements.Select(x => x.Entitlement));

            var networkEntitlement = mapper.Map<NetworkEntitlement>(updateNetworkEntitlementDto);
            networkEntitlement.NetworkId = networkId;

            networkEntitlement = await entitlementRepository.UpdateAsync(networkEntitlement);
            return networkEntitlement;
        }

        private static void ValidateGlobalLimits(
            Guid updatingNetworkId,
            UpdateNetworkEntitlementDto incoming,
            NetworkEntitlement mainLimit,
            IEnumerable<NetworkEntitlement> allEntitlements)
        {
            var otherNetworks = allEntitlements
                .Where(e => e.NetworkId != updatingNetworkId && e.NetworkId != mainLimit.NetworkId);

            // Validation Logic: (Sum of Others) + (Incoming Value) <= (Main Network Limit)

            var networkEntitlements = otherNetworks as NetworkEntitlement[] ?? otherNetworks.ToArray();
            if ((networkEntitlements.Sum(e => e.FileCountLimit) + incoming.FileCountLimit) > mainLimit.FileCountLimit)
                throw new BadHttpRequestException("File count limit exceeds global capacity.");

            if ((networkEntitlements.Sum(e => e.FileStorageLimit) + incoming.FileStorageLimit) >
                mainLimit.FileStorageLimit)
                throw new BadHttpRequestException("Total file storage exceeds global capacity.");

            if ((networkEntitlements.Sum(e => e.BlogCountLimit) + incoming.BlogCountLimit) > mainLimit.BlogCountLimit)
                throw new BadHttpRequestException("Blog count limit exceeds global capacity.");

            if ((networkEntitlements.Sum(e => e.ConfigurationCountLimit) + incoming.ConfigurationCountLimit) >
                mainLimit.ConfigurationCountLimit)
                throw new BadHttpRequestException("Configuration count limit exceeds global capacity.");

            if ((networkEntitlements.Sum(e => e.CustomPageCountLimit) + incoming.CustomPageCountLimit) >
                mainLimit.CustomPageCountLimit)
                throw new BadHttpRequestException("Custom page count limit exceeds global capacity.");

            if ((networkEntitlements.Sum(e => e.CustomPageBlockCountLimit) + incoming.CustomPageBlockCountLimit) >
                mainLimit.CustomPageBlockCountLimit)
                throw new BadHttpRequestException("Custom page block count limit exceeds global capacity.");

            // Add additional checks for when adding new
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
                await cdnService.DeleteAsync(networkFile.Bucket, networkFile.ObjectKey);
            }

            await RefreshIssuerCacheAsync();

            await repository.DeleteAsync(network);
        }

        public async Task<Network> GetMainNetworkAsync()
        {
            if (_cachedMainNetwork != null)
                return _cachedMainNetwork;

            _cachedMainNetwork = await repository.GetMainNetworkAsync();
            return _cachedMainNetwork;
        }

        public async Task<Network> GetMainNetworkDetailsAsync()
        {
            var network = await repository.GetMainNetworkDetailsAsync();
            return network;
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
        
        private async Task RefreshIssuerCacheAsync()
        {
            var allNetworks = await repository.GetAllAsync();
            var redirectUris = allNetworks.Select(n => n.RedirectURI);
            audienceCache.ReloadAudiences(redirectUris);
        }
    }
}