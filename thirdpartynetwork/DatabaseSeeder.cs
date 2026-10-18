using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Helpers;
using File = thirdpartynetwork.Core.Models.File;
using Role = thirdpartynetwork.Core.Models.Role;


namespace thirdpartynetwork
{
    public class DatabaseSeeder(DataContext context, IPasswordHasher passwordHasher, ICDNService cdnService)
    {
        public async Task SeedAsync()
        {
            var forceCleanSeed = false;
            // forceCleanSeed = true; // Easily comment or uncomment this line to toggle.

            if (forceCleanSeed || !context.Networks.Any())
            {
                await CleanSeedAsync();
            }

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" &&
                context.Networks.FirstOrDefault(x => x.Name == "Dummy Network") == null)
            {
                await DevSeedAsync();
            }
        }

        private async Task DevSeedAsync()
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var dummyNetwork = new Network
                {
                    Name = "Dummy Network",
                    Description = "Dummy Network Description",
                    IsPublic = true,
                    RedirectURI = "#",
                    FeatureFlag = new ()
                    {
                        AllowBlogs = false,
                        AllowConfigurations = false,
                        AllowFiles = false,
                        AllowCustomPages = false
                    }
                };

                var adminRole = new Role
                {
                    Name = "Dummy Admin",
                    Description = "Admin Role",
                    Network = dummyNetwork,
                    IsDefault = false,
                    FeatureFlags = new RoleFeatureFlag()
                };

                adminRole.RolePermissions = new List<RolePermission>()
                {
                    new()
                    {
                        Role = adminRole,
                        Permission =
                            context.Permissions.FirstOrDefault(x => x.Name == Constants.Permissions.Administrator)!
                    }
                };

                var userRole = new Role
                {
                    Name = "Dummy User",
                    Description = "User Role",
                    Network = dummyNetwork,
                    IsDefault = true,
                    FeatureFlags = new RoleFeatureFlag()
                };

                userRole.RolePermissions = new List<RolePermission>()
                {
                    
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadNetwork),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadPermission),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadAccess),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadFile),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadRole),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadCustomPage),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = context.Permissions.First(p => p.Name == Constants.Permissions.ReadPageBlock),
                        Role = userRole
                    },
                };

                dummyNetwork.Roles = new List<Role>()
                {
                    adminRole,
                    userRole
                };
                
                var mainNetwork = context.Networks.FirstOrDefault(x => x.Name == Constants.Networks.Main)!;

                var dummyAdmin = new User() { };

                var dummyAdminProxy = new UserProxy()
                {
                    User = dummyAdmin,
                    IsDefault = true,
                    FirstName = "Dummy A",
                    LastName = "Admin",
                    Username = "AdminDummy",
                    Email = "dummy@admin.com",
                    Password = passwordHasher.HashPassword("dummy@admin.com"),
                };
                
                dummyAdmin.UserProxies.Add(dummyAdminProxy);

                var dummyNetworkAdmin = new NetworkUser()
                {
                    UserProxy = dummyAdminProxy,
                    Network = dummyNetwork,
                    FeatureFlags = new NetworkUserFeatureFlag()
                };
                
                var nu1 = new NetworkUser()
                {
                    UserProxy = dummyAdminProxy,
                    Network = mainNetwork,
                    FeatureFlags = new NetworkUserFeatureFlag()
                };

                dummyAdminProxy.NetworkUsers = new List<NetworkUser>()
                {
                    dummyNetworkAdmin,
                    nu1,
                };

                dummyNetworkAdmin.NetworkUserRoles = new List<NetworkUserRole>()
                {
                    new()
                    {
                        Role = adminRole,
                        NetworkUser = dummyNetworkAdmin,
                    }
                };

                nu1.NetworkUserRoles = new List<NetworkUserRole>()
                {
                    new()
                    {
                        Role = context.Roles.FirstOrDefault(x => x.Name == Constants.Roles.User)!,
                        NetworkUser = dummyNetworkAdmin,
                    }
                };

                var dummyUser = new User() { };

                var dummyUserProxy = new UserProxy()
                {
                    User = dummyUser,
                    IsDefault = true,
                    FirstName = "Dummy U",
                    LastName = "User",
                    Username = "UserDummy",
                    Email = "dummy@user.com",
                    Password = passwordHasher.HashPassword("dummy@user.com"),
                };
                
                dummyUser.UserProxies.Add(dummyUserProxy);

                var dummyNetworkUser = new NetworkUser()
                {
                    UserProxy = dummyUserProxy,
                    Network = dummyNetwork,
                    FeatureFlags = new NetworkUserFeatureFlag()
                };
                
                var nu2 = new NetworkUser()
                {
                    UserProxy = dummyUserProxy,
                    Network = mainNetwork,
                    FeatureFlags = new NetworkUserFeatureFlag()
                };

                dummyUserProxy.NetworkUsers = new List<NetworkUser>()
                {
                    dummyNetworkUser,
                    nu2,
                };

                dummyNetworkUser.NetworkUserRoles = new List<NetworkUserRole>()
                {
                    new()
                    {
                        Role = userRole,
                        NetworkUser = dummyNetworkUser,
                    }
                };

                nu2.NetworkUserRoles = new List<NetworkUserRole>()
                {
                    new()
                    {
                        Role = context.Roles.FirstOrDefault(x => x.Name == Constants.Roles.User)!,
                        NetworkUser = dummyNetworkUser,
                    }
                };

                dummyNetwork.NetworkAccesses = new List<NetworkAccess>()
                {
                    new()
                    {
                        Access = context.Accesses.FirstOrDefault(x => x.Name == Constants.Access.AccessUsername)!,
                        Network = dummyNetwork,
                        IsRequired = true,
                    }
                };

                context.AddRange([dummyNetwork, dummyAdmin, dummyUser]);

                await context.SaveChangesAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await transaction.CommitAsync();
        }

        private async Task CleanSeedAsync()
        {
            await ClearDatabase();

            // Start transaction for safe rollback
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var network = new Network
                {
                    Name = Constants.Networks.Main,
                    Description = "TrojoNetworks' main network",
                    ImageFile = null,
                    RedirectURI = "#",
                    IsSystemProtected = true,
                    IsPublic = true,
                    FeatureFlag = new ()
                    {
                        AllowBlogs = true,
                        AllowConfigurations = true,
                        AllowFiles = true,
                        AllowCustomPages = true,
                    }
                };

                var permissions = new List<Permission>
                {
                    new()
                    {
                        Name = Constants.Permissions.Administrator,
                        Description = "User has ALL permissions and can do ANYTHING.", IsSystemProtected = true
                    },
                    new() { Name = Constants.Permissions.ReadNetwork, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageNetwork, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadAccess, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageAccess, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadPermission, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManagePermission, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadRole, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageRole, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadUser, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageUser, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadCustomPage, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageCustomPage, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadPageBlock, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManagePageBlock, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadFile, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageFile, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadConfiguration, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageConfiguration, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ReadBlog, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageBlog, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ManageContentAccess, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ModerateFile, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ModerateBlog, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ModerateConfiguration, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ModerateCustomPage, Description = "", IsSystemProtected = true },
                    new() { Name = Constants.Permissions.ModeratePageBlock, Description = "", IsSystemProtected = true },
                };

                var accesses = new List<Access>
                {
                    new()
                    {
                        Name = Constants.Access.AccessUsername,
                        Description = "Allows Network to Access User's Username", 
                        IsSystemProtected = true
                    },
                    new()
                    {
                        Name = Constants.Access.AccessFirstname,
                        Description = "Allows Network to Access User's Firstname", 
                        IsSystemProtected = true
                    },
                    new()
                    {
                        Name = Constants.Access.AccessLastname,
                        Description = "Allows Network to Access User's Lastname", 
                        IsSystemProtected = true
                    },
                    new()
                    {
                        Name = Constants.Access.AccessEmail,
                        Description = "Allows Network to Access User's Email",
                        IsSystemProtected = true
                    },
                    new()
                    {
                        Name = Constants.Access.AccessProfilePicture,
                        Description = "Allows Network to Access User's ProfilePicture", 
                        IsSystemProtected = true
                    }
                };

                var adminRole = new Role
                {
                    Name = Constants.Roles.Admin,
                    Description = "Application Administrator",
                    IsSystemProtected = true,
                    Network = network,
                    IsDefault = false,
                    FeatureFlags = new RoleFeatureFlag()
                };

                adminRole.RolePermissions = new List<RolePermission>
                {
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.Administrator),
                        Role = adminRole
                    }
                };

                var userRole = new Role
                {
                    Name = Constants.Roles.User,
                    Description = "Application User",
                    IsSystemProtected = true,
                    Network = network,
                    IsDefault = true,
                    FeatureFlags = new RoleFeatureFlag()
                };

                userRole.RolePermissions = new List<RolePermission>()
                {
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadNetwork),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadPermission),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadAccess),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadFile),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadRole),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadCustomPage),
                        Role = userRole
                    },
                    new()
                    {
                        Permission = permissions.First(p => p.Name == Constants.Permissions.ReadPageBlock),
                        Role = userRole
                    },
                };

                var adminUser = new UserProxy
                {
                    Username = "Admin",
                    FirstName = "Admin",
                    LastName = "Noodle",
                    Email = "admin@gmail.com",
                    Password = passwordHasher.HashPassword(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ? "admin@gmail.com" : "LittleNoodleBigNoodle"),
                    ImageFile = null,
                    IsDefault = true,
                    User = new User()
                };

                var adminNetworkUser = new NetworkUser
                {
                    UserProxy = adminUser,
                    Network = network,
                    FeatureFlags = new NetworkUserFeatureFlag()
                };

                adminNetworkUser.NetworkUserRoles = new List<NetworkUserRole>
                {
                    new() { Role = adminRole, NetworkUser = adminNetworkUser },
                    new() { Role = userRole, NetworkUser = adminNetworkUser }
                };

                adminNetworkUser.NetworkUserAccesses = accesses.Select(access => new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = access,
                    IsAccepted = true
                }).ToList();

                network.NetworkAccesses = accesses.Select(access => new NetworkAccess
                {
                    Access = access,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true
                }).ToList();

                network.NetworkUsers = new List<NetworkUser> { adminNetworkUser };

                context.Networks.Add(network);
                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await transaction.CommitAsync();
        }

        private async Task ClearDatabase()
        {
            Console.WriteLine("Clearing database.");

            Console.WriteLine("Clearing all files from MinIO.");
            await cdnService.ClearAllFilesAsync();

            var dbSets = context.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.Name)
                .ToList();

            var tables = string.Join(", ", dbSets.Select(t => $"\"{t}\""));

            var sql = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE;";
            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}