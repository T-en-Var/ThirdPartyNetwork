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
        IIdentityVerifier identityVerifier) : INetworkService
    {
        private ICollection<Network> FilterNetworks(ICollection<Network> networks, bool bypass = false)
        {
            var currentUser = identityService.GetUserProxy(bypass);
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
            Console.WriteLine("Getting (service) network with id: " + id);
            Console.WriteLine("Getting (service) network with id: " + id);
            Console.WriteLine("Getting (service) network with id: " + id);
            var network = repository.GetById(id, bypass);
            if (network == null) return null;

            var networks = FilterNetworks([network], bypass);
            return networks.Count > 0 ? networks.First() : null;
        }

        public Network? GetDetailsById(Guid id)
        {
            Console.WriteLine("Getting (service) network details with id: " + id);
            Console.WriteLine("Getting (service) network details with id: " + id);
            Console.WriteLine("Getting (service) network details with id: " + id);
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
            var adminPermission = permissionService.GetByName(Constants.Permissions.Administrator);

            if (adminPermission == null) throw new Exception("Admin permission not found, this is NOT good.");

            adminRole.RolePermissions =
            [
                new RolePermission
                {
                    Role = null,
                    Permission = adminPermission
                }
            ];

            var currentUser = identityService.GetUserProxy();
            Console.WriteLine(currentUser.Username);
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
                            Role = userRole
                        },

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

            network = repository.Update(network);
            return network;
        }

        public bool DeleteById(Guid id)
        {
            var network = GetById(id);
            return network != null && repository.Delete(network);
        }

        public Network GetMainNetwork()
        {
            var mainNetwork = repository.GetMainNetwork();

            return mainNetwork;
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