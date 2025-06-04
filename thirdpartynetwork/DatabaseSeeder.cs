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
                Description = "Thirdpartynetwork's main network",
                ImageUrl = "/src/images/logo_transparent.png",
                RedirectURI = "#",
                IsSystemProtected = true,
                IsPublic = true,
            };

            Permission administrator = new()
            {
                Name = Constants.Permissions.Administrator,
                Description = "User has ALL permissions and can do ANYTHING.",
                IsSystemProtected = true
            };
            Permission readNetwork = new()
            {
                Name = Constants.Permissions.ReadNetwork,
                Description = "",
                IsSystemProtected = true,
            };
            Permission manageNetwork = new()
            {
                Name = Constants.Permissions.ManageNetwork,
                Description = "",
                IsSystemProtected = true,
            };
            Permission readAccess = new()
            {
                Name = Constants.Permissions.ReadAccess,
                Description = "",
                IsSystemProtected = true,
            };
            Permission manageAccess = new()
            {
                Name = Constants.Permissions.ManageAccess,
                Description = "",
                IsSystemProtected = true,
            };
            Permission readPermission = new()
            {
                Name = Constants.Permissions.ReadPermission,
                Description = "",
                IsSystemProtected = true,
            };
            Permission managePermission = new()
            {
                Name = Constants.Permissions.ManagePermission,
                Description = "",
                IsSystemProtected = true,
            };
            Permission readRole = new()
            {
                Name = Constants.Permissions.ReadRole,
                Description = "",
                IsSystemProtected = true,
            };
            Permission manageRole = new()
            {
                Name = Constants.Permissions.ManageRole,
                Description = "",
                IsSystemProtected = true,
            };
            Permission readUser = new()
            {
                Name = Constants.Permissions.ReadUser,
                Description = "",
                IsSystemProtected = true,
            };
            Permission manageUser = new()
            {
                Name = Constants.Permissions.ManageUser,
                Description = "",
                IsSystemProtected = true,
            };
            Permission readContent = new()
            {
                Name = Constants.Permissions.ReadContent,
                Description = "",
                IsSystemProtected = true,
            };
            Permission manageContent = new()
            {
                Name = Constants.Permissions.ManageContent,
                Description = "",
                IsSystemProtected = true,
            };

            // Permission kickUser = new()
            // {
            //     Name = Constants.Permissions.KickUser, 
            //     Description = "Allows user to kick lower ranked users.",
            //     IsSystemProtected = true
            // };
            // Permission kickUserAdmin = new()
            // {
            //     Name = Constants.Permissions.KickUserAdmin, 
            //     Description = "Allows user to kick any user.",
            //     IsSystemProtected = true
            // };
            // Permission banUser = new()
            // {
            //     Name = Constants.Permissions.BanUser, 
            //     Description = "Allows user to ban lower ranked users.",
            //     IsSystemProtected = true
            // };
            // Permission banUserAdmin = new()
            // {
            //     Name = Constants.Permissions.BanUserAdmin, 
            //     Description = "Allows user to ban any user.",
            //     IsSystemProtected = true
            // };
            // Permission timeoutUser = new()
            // {
            //     Name = Constants.Permissions.TimoutUser, 
            //     Description = "Allows user to timeout lower ranked users.",
            //     IsSystemProtected = true
            // };
            // Permission timeoutUserAdmin = new()
            // {
            //     Name = Constants.Permissions.TimoutUserAdmin, 
            //     Description = "Allows user to timeout any user.",
            //     IsSystemProtected = true
            // };

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
                IsDefault = false,
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
                IsDefault = true,
            };

            userRole.RolePermissions =
            [
                new RolePermission() { Permission = readContent, Role = userRole },
            ];

            UserProxy adminUser = new()
            {
                Username = "Admin",
                FirstName = "Admin",
                LastName = "Noodle",
                Email = "admin.noodle@gmail.com",
                Password = passwordHasher.HashPassword("Admin"),
                ImageUrl =
                    "https://cdn.discordapp.com/avatars/196606443703631874/a_336c3be4f4e5c1ba3563c74a7cc065da.gif?size=1024",
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
                    IsRequired = false,
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
                    IsRequired = false,
                    IsSystemProtected = true,
                },
            ];

            network.NetworkUsers = [adminNetworkUser];

            context.Networks.Add(network);
            context.Permissions.AddRange(
                readNetwork,
                manageNetwork,
                readAccess,
                manageAccess,
                readPermission,
                managePermission,
                readRole,
                manageRole,
                readUser,
                manageUser,
                readContent,
                manageContent
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