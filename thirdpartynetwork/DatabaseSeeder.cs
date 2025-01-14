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

            network.Roles = [adminRole, userRole];

            Access accessUsername = new() { Name = Constants.Access.AccessUsername, Description = "Allows Network to Access User's Username", IsSystemProtected = true };
            Access accessFirstname = new() { Name = Constants.Access.AccessFirstname, Description = "Allows Network to Access User's Firstname", IsSystemProtected = true };
            Access accessLastname = new() { Name = Constants.Access.AccessLastname, Description = "Allows Network to Access User's Lastname", IsSystemProtected = true };
            Access accessEmail = new() { Name = Constants.Access.AccessEmail, Description = "Allows Network to Access User's Email", IsSystemProtected = true };
            Access accessProfilePicture = new() { Name = Constants.Access.AccessProfilePicture, Description = "Allows Network to Access User's ProfilePicture", IsSystemProtected = true };

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

            adminNetworkUser.NetworkUserAccesses =
            [
                new NetworkUserAccess
                {
                    NetworkUser = adminNetworkUser,
                    Access = accessUsername,
                    IsAccepted = true,
                } ,
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
