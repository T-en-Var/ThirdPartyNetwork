using AutoMapper;
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
        IIdentityVerifier identityVerifier) : INetworkService
    {
        private static Network? _cachedMainNetwork;

        private ICollection<Network> FilterNetworks(ICollection<Network> networks, bool bypass = false)
        {
            if (bypass) return networks;

            var currentUser = identityService.GetUserProxy();
            var isSuper =
                identityVerifier.IsSuperAdminOrHasSuperPermission(currentUser.Id, Constants.Permissions.ReadNetwork);

            return networks
                .Where(network => network.IsPublic
                                  || isSuper
                                  || identityVerifier.IsAllowed(network.Id, currentUser.Id,
                                      Constants.Permissions.ReadNetwork))
                .ToList();
        }

        public ICollection<Network> GetAll()
        {
            var networks = repository.GetAll();
            return FilterNetworks(networks);
        }

        public Network? GetById(Guid id, bool bypass = false)
        {
            var network = repository.GetById(id, bypass);
            if (network == null) return null;

            var networks = FilterNetworks([network], bypass);
            return networks.Count > 0 ? networks.First() : null;
        }

        public Network? GetDetailsById(Guid id)
        {
            var network = repository.GetDetailsById(id);
            if (network == null) return null;

            var currentUser = identityService.GetUserProxy(omitThrow: true);

            network.NetworkUsers = network.NetworkUsers
                .Where(x => x.UserProxyId == currentUser.Id).ToList();

            return network;
        }

        public Network Create(CreateNetworkDto createNetworkDto)
        {
            var nameFound = repository.GetByName(createNetworkDto.Name);
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
                    Permission = permissionService.GetByName(Constants.Permissions.Administrator)!
                }
            ];

            userRole.RolePermissions = new List<RolePermission>()
            {
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadNetwork)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadPermission)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadAccess)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadFile)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadRole)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadCustomPage)!,
                    Role = userRole
                },
                new()
                {
                    Permission = permissionService.GetByName(Constants.Permissions.ReadPageBlock)!,
                    Role = userRole
                },
            };

            var currentUser = identityService.GetUserProxy();
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
            network = repository.Create(network);

            return network;
        }

        public Network Update(Guid id, UpdateNetworkDto updateNetworkDto)
        {
            if (!repository.Exists(id)) throw new NotFoundHttpException();

            var network = mapper.Map<Network>(updateNetworkDto);
            network.Id = id;

            if (updateNetworkDto.FileLink != null)
            {
                network.ImageFileId = updateNetworkDto.FileLink.Id;
            }

            network = repository.Update(network);
            return network;
        }

        public async Task DeleteById(Guid id)
        {
            var network = GetById(id, true);
            if (network == null) return;

            network.ImageFileId = null;
            network.ImageFile = null;

            foreach (var networkFile in network.Files)
            {
                await cdnService.DeleteAsync($"{networkFile.NetworkId}/{networkFile.PublicId}", networkFile.MediaType);
            }

            repository.Delete(network);
        }

        public Network GetMainNetwork()
        {
            if (_cachedMainNetwork != null)
                return _cachedMainNetwork;

            _cachedMainNetwork = repository.GetMainNetwork();
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