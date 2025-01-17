using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Helpers;

namespace thirdpartynetwork
{
    public class DatabaseSeeder(DataContext context, IPasswordHasher passwordHasher)
    {
        public async Task SeedAsync()
        {
            if (context.Networks.Any()) return;
            ClearDatabase();

            Network network = new()
            {
                Name = Constants.Networks.Main,
                IsSystemProtected = true,
            };

            Permission assignUserRoles = new()
            {
                Name = Constants.Permissions.AssignUserRoles,
                Description = "Allows user to assign (lower) roles to self and lower ranked.", 
                IsSystemProtected = true
            };
            Permission assignAdminRoles = new()
            {
                Name = Constants.Permissions.AssignAdminRoles,
                Description = "Allows user to assign (higher) roles to self, same rank or higher.",
                IsSystemProtected = true
            };
            Permission administrator = new()
            {
                Name = Constants.Permissions.Administrator,
                Description = "User has ALL permissions and can do ANYTHING.", 
                IsSystemProtected = true
            };
            Permission updateSelf = new()
            {
                Name = Constants.Permissions.UpdateSelf, 
                Description = "Allows user to edit self.",
                IsSystemProtected = true
            };
            Permission updateUser = new()
            {
                Name = Constants.Permissions.UpdateUser,
                Description = "Allows user to update other users within the network.", 
                IsSystemProtected = true
            };
            Permission createContent = new()
            {
                Name = Constants.Permissions.CreateContent, 
                Description = "Allows user to create content.",
                IsSystemProtected = true
            };
            Permission editContent = new()
            {
                Name = Constants.Permissions.EditContent, 
                Description = "Allows user to edit any content.",
                IsSystemProtected = true
            };
            Permission editOwnContent = new()
            {
                Name = Constants.Permissions.EditOwnContent, 
                Description = "Allows user to edit own content.",
                IsSystemProtected = true
            };
            Permission deleteContent = new()
            {
                Name = Constants.Permissions.DeleteContent, 
                Description = "Allows user to delete any content.",
                IsSystemProtected = true
            };
            Permission deleteOwnContent = new()
            {
                Name = Constants.Permissions.DeleteOwnContent, 
                Description = "Allows user to delete own content.",
                IsSystemProtected = true
            };
            Permission viewContent = new()
            {
                Name = Constants.Permissions.ViewContent, 
                Description = "Allows user to view any content.",
                IsSystemProtected = true
            };
            Permission viewOwnContent = new()
            {
                Name = Constants.Permissions.ViewOwnContent, 
                Description = "Allows user to view own content.",
                IsSystemProtected = true
            };
            Permission editNetwork = new()
            {
                Name = Constants.Permissions.EditNetwork, 
                Description = "Allows user to edit network values.",
                IsSystemProtected = true
            };
            Permission editNetworkRoles = new()
            {
                Name = Constants.Permissions.EditNetworkRoles,
                Description = "Allows user to edit network roles and permissions.", 
                IsSystemProtected = true
            };
            Permission kickUser = new()
            {
                Name = Constants.Permissions.KickUser, 
                Description = "Allows user to kick lower ranked users.",
                IsSystemProtected = true
            };
            Permission kickUserAdmin = new()
            {
                Name = Constants.Permissions.KickUserAdmin, 
                Description = "Allows user to kick any user.",
                IsSystemProtected = true
            };
            Permission banUser = new()
            {
                Name = Constants.Permissions.BanUser, 
                Description = "Allows user to ban lower ranked users.",
                IsSystemProtected = true
            };
            Permission banUserAdmin = new()
            {
                Name = Constants.Permissions.BanUserAdmin, 
                Description = "Allows user to ban any user.",
                IsSystemProtected = true
            };
            Permission timeoutUser = new()
            {
                Name = Constants.Permissions.TimoutUser, 
                Description = "Allows user to timeout lower ranked users.",
                IsSystemProtected = true
            };
            Permission timeoutUserAdmin = new()
            {
                Name = Constants.Permissions.TimoutUserAdmin, 
                Description = "Allows user to timeout any user.",
                IsSystemProtected = true
            };
            Permission editNetworkAccesses = new()
            {
                Name = Constants.Permissions.EditNetworkAccesses,
                Description = "Allows user to edit network accesses list.",
                IsSystemProtected = true
            };
            Permission createNetwork = new()
            {
                Name = Constants.Permissions.CreateNetwork,
                Description = "Allows user to create a new network.",
                IsSystemProtected = true
            };

            Access accessUsername = new()
            {
                Name = Constants.Access.AccessUsername, 
                Description = "Allows Network to Access User's Username",
                IsSystemProtected = true
            };
            Access accessFirstname = new()
            {
                Name = Constants.Access.AccessFirstname, 
                Description = "Allows Network to Access User's Firstname",
                IsSystemProtected = true
            };
            Access accessLastname = new()
            {
                Name = Constants.Access.AccessLastname, 
                Description = "Allows Network to Access User's Lastname",
                IsSystemProtected = true
            };
            Access accessEmail = new()
            {
                Name = Constants.Access.AccessEmail, 
                Description = "Allows Network to Access User's Email",
                IsSystemProtected = true
            };
            Access accessProfilePicture = new()
            {
                Name = Constants.Access.AccessProfilePicture,
                Description = "Allows Network to Access User's ProfilePicture", 
                IsSystemProtected = true
            };

            Role adminRole = new()
            {
                Name = Constants.Roles.Admin,
                Description = "Application Administrator",
                IsSystemProtected = true,
                Network = network,
            };

            adminRole.RolePermissions =
            [
                new RolePermission() { Permission = administrator, Role = adminRole }
            ];
            
            Role userRole = new()
            {
                Name = Constants.Roles.User,
                Description = "Application User",
                IsSystemProtected = true,
                Network = network,
            };
            
            userRole.RolePermissions =
            [
                new RolePermission() { Permission = updateSelf, Role = userRole },
                new RolePermission() { Permission = viewContent, Role = userRole },
            ];

            Role clientRole = new()
            {
                Name = Constants.Roles.Client,
                Description = "Client who may create and manage networks.",
                IsSystemProtected = true,
                Network = network,
            };

            clientRole.RolePermissions =
            [
                new RolePermission() { Permission = createNetwork, Role = clientRole },
            ];

            UserProxy adminUser = new()
            {
                Username = "Admin",
                FirstName = "Admin",
                LastName = "Noodle",
                Email = "admin.noodle@gmail.com",
                Password = passwordHasher.HashPassword("Admin"),
                ProfilePicture = "https://cdn.discordapp.com/avatars/196606443703631874/a_336c3be4f4e5c1ba3563c74a7cc065da.gif?size=1024",
                IsDefault = true,
                User = new User()
            };

            NetworkUser adminNetworkUser = new()
            {
                UserProxy = adminUser,
                UserProxyId = adminUser.Id,
                Network = network,
                NetworkId = network.Id,
            };

            adminNetworkUser.NetworkUserRoles =
            [
                new NetworkUserRole
                {
                    Role = adminRole,
                    NetworkUser = adminNetworkUser,
                },
                new NetworkUserRole
                {
                    Role = userRole,
                    NetworkUser = adminNetworkUser
                }
            ];

            adminNetworkUser.NetworkUserAccesses =
            [
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessUsername,
                    IsAccepted = true,
                },
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessFirstname,
                    IsAccepted = true,
                },
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessLastname,
                    IsAccepted = true,
                },
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessEmail,
                    IsAccepted = true,
                },
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessProfilePicture,
                    IsAccepted = true,
                },
            ];

            network.NetworkAccesses =
            [
                new NetworkAccess
                {
                    Access = accessUsername,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkAccess
                {
                    Access = accessFirstname,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkAccess
                {
                    Access = accessLastname,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkAccess
                {
                    Access = accessEmail,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkAccess
                {
                    Access = accessProfilePicture,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
            ];

            network.NetworkUsers = [adminNetworkUser];

            context.Networks.Add(network);
            context.Permissions.AddRange(
                assignUserRoles,
                assignAdminRoles,
                updateUser,
                createContent,
                editContent,
                editOwnContent,
                deleteContent,
                deleteOwnContent,
                viewOwnContent,
                editNetwork,
                editNetworkRoles,
                kickUser,
                kickUserAdmin,
                banUser,
                banUserAdmin,
                timeoutUser,
                timeoutUserAdmin,
                editNetworkAccesses
            );
            await context.SaveChangesAsync();
        }

        private void ClearDatabase()
        {
            Console.WriteLine("Clearing database.");

            var dbSets = context.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.Name)
                .ToList();

            var tables = string.Join(", ", dbSets.Select(tableName => $"\"{tableName}\""));
            var truncateCommand = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE;";

            context.Database.ExecuteSqlRaw(truncateCommand);
        }
    }
}