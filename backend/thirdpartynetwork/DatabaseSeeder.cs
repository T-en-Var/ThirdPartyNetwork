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
            // if (context.Networks.Any()) return;
            ClearDatabase();

            Network network = new()
            {
                Name = Constants.Networks.Main,
                IsSystemProtected = true,
            };

            Role adminRole = new() { Name = Constants.Roles.Admin, Description = "Application Administrator", IsSystemProtected = true };
            Role userRole = new() { Name = Constants.Roles.User, Description = "Application User", IsSystemProtected = true };

            network.NetworkRoles =
            [
                new NetworkRole()
                {
                    Role = adminRole,
                    Network = network,
                    IsSystemProtected = true,
                },
                new NetworkRole()
                {
                    Role = userRole,
                    Network = network,
                    IsSystemProtected = true,
                }
            ];

            Permission accessUsername = new() { Name = Constants.Permissions.AccessUsername, Description = "Allows Network to Access User's Username", IsSystemProtected = true };
            Permission accessFirstname = new() { Name = Constants.Permissions.AccessFirstname, Description = "Allows Network to Access User's Firstname", IsSystemProtected = true };
            Permission accessLastname = new() { Name = Constants.Permissions.AccessLastname, Description = "Allows Network to Access User's Lastname", IsSystemProtected = true };
            Permission accessEmail = new() { Name = Constants.Permissions.AccessEmail, Description = "Allows Network to Access User's Email", IsSystemProtected = true };
            Permission accessProfilePicture = new() { Name = Constants.Permissions.AccessProfilePicture, Description = "Allows Network to Access User's ProfilePicture", IsSystemProtected = true };

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
                    NetworkUser = adminNetworkUser
                },
                new NetworkUserRole
                {
                    Role = userRole,
                    NetworkUser = adminNetworkUser
                }
            ];

            adminNetworkUser.NetworkUserPermissions =
            [
                new NetworkUserPermission
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessUsername,
                    IsAccepted = true,
                } ,
                new NetworkUserPermission
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessFirstname,
                    IsAccepted = true,
                },
                new NetworkUserPermission
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessLastname,
                    IsAccepted = true,
                },
                new NetworkUserPermission
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessEmail,
                    IsAccepted = true,
                },
                new NetworkUserPermission
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessProfilePicture,
                    IsAccepted = true,
                },
            ];

            network.NetworkPermissions =
            [
                new NetworkPermission
                {
                    Permission = accessUsername,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkPermission
                {
                    Permission = accessFirstname,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkPermission
                {
                    Permission = accessLastname,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkPermission
                {
                    Permission = accessEmail,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
                new NetworkPermission
                {
                    Permission = accessProfilePicture,
                    Network = network,
                    IsRequired = true,
                    IsSystemProtected = true,
                },
            ];

            network.NetworkUsers = [ adminNetworkUser ];

            context.Networks.Add(network);
            await context.SaveChangesAsync();
        }

        private void ClearDatabase()
        {
            Console.WriteLine("Clearing database.");

            var dbSets = context.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.Name)
                .ToList();

            var tables = string.Join(", ", dbSets.Select(tableName => $"\"{tableName}\""));
            var truncateCommand = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE;";

            context.Database.ExecuteSqlRaw(truncateCommand);
        }
    }
}