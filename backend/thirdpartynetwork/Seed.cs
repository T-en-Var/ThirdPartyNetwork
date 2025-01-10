using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;

namespace thirdpartynetwork
{
    public class Seed(DataContext context)
    {
        public void SeedDataContext()
        {
            ClearDatabase();

            Network network = new()
            {
                Name = "thirdpartynetwork"
            };

            Role adminRole = new() { Name = "Admin" };
            Role userRole = new() { Name = "User" };

            network.NetworkRoles =
            [
                new NetworkRole()
                {
                    Role = adminRole,
                    Network = network
                },
                new NetworkRole()
                {
                    Role = userRole,
                    Network = network
                }
            ];

            Permission accessUsername = new() { Name = "Access Username", Description = "Allows Network to Access User's Username" };
            Permission accessFirstname = new() { Name = "Access Firstname", Description = "Allows Network to Access User's Firstname" };
            Permission accessLastname = new() { Name = "Access Lastname", Description = "Allows Network to Access User's Lastname" };
            Permission accessEmail = new() { Name = "Access Email", Description = "Allows Network to Access User's Email" };
            Permission accessProfilePicture = new() { Name = "Access ProfilePicture", Description = "Allows Network to Access User's ProfilePicture" };

            UserProxy adminUser = new()
            {
                Username = "Admin",
                FirstName = "Admin",
                LastName = "Noodle",
                Email = "admin.noodle@gmail.com",
                Password = "Admin",
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
                },
                new NetworkPermission
                {
                    Permission = accessFirstname,
                    Network = network,
                    IsRequired = true,
                },
                new NetworkPermission
                {
                    Permission = accessLastname,
                    Network = network,
                    IsRequired = true,
                },
                new NetworkPermission
                {
                    Permission = accessEmail,
                    Network = network,
                    IsRequired = true,
                },
                new NetworkPermission
                {
                    Permission = accessProfilePicture,
                    Network = network,
                    IsRequired = true,
                },
            ];

            network.NetworkUsers = [ adminNetworkUser ];

            context.Networks.Add(network);
            context.SaveChanges();
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