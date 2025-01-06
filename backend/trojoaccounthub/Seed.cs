using Microsoft.EntityFrameworkCore;
using trojoaccounthub.Data;
using trojoaccounthub.Models;

namespace trojoaccounthub
{
    public class Seed
    {
        private readonly DataContext dataContext;
        public Seed(DataContext context)
        {
            this.dataContext = context;
        }

        public void SeedDataContext()
        {
            ClearDatabase();

            Network network = new()
            {
                Name = "trojoaccounthub"
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
            Permission accessLastename = new() { Name = "Access Lastename", Description = "Allows Network to Access User's Lastename" };
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

            Models.NetworkUser adminNetworkUser = new()
            {
                UserProxy = adminUser,
                UserProxyId = adminUser.Id,
                Network = network,
                NetworkId = network.Id,
            };

            adminNetworkUser.NetworkUserRoles =
            [
                new()
                {
                    Role = adminRole,
                    NetworkUser = adminNetworkUser
                },
                new()
                {
                    Role = userRole,
                    NetworkUser = adminNetworkUser
                }
            ];

            adminNetworkUser.NetworkUserPermissions =
            [
                new() 
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessUsername,
                    IsAccepted = true,
                } ,
                new() 
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessFirstname,
                    IsAccepted = true,
                },
                new() 
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessLastename,
                    IsAccepted = true,
                },
                new() 
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessEmail,
                    IsAccepted = true,
                },
                new() 
                {
                    NetworkUser = adminNetworkUser,
                    Permission = accessProfilePicture,
                    IsAccepted = true,
                },
            ];

            network.NetworkPermissions =
            [
                new()
                {
                    Permission = accessUsername,
                    Network = network,
                    IsRequired = true,
                },
                new()
                {
                    Permission = accessFirstname,
                    Network = network,
                    IsRequired = true,
                },
                new()
                {
                    Permission = accessLastename,
                    Network = network,
                    IsRequired = true,
                },
                new()
                {
                    Permission = accessEmail,
                    Network = network,
                    IsRequired = true,
                },
                new()
                {
                    Permission = accessProfilePicture,
                    Network = network,
                    IsRequired = true,
                },
            ];

            network.NetworkUsers = [ adminNetworkUser ];

            dataContext.Networks.Add(network);
            dataContext.SaveChanges();
        }

        public void ClearDatabase()
        {
            Console.WriteLine("Clearing database.");

            var dbSets = dataContext.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.Name)
                .ToList();

            string tables = string.Join(", ", dbSets.Select(tableName => $"\"{tableName}\""));
            string truncateCommand = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE;";

            dataContext.Database.ExecuteSqlRaw(truncateCommand);
        }
    }
}