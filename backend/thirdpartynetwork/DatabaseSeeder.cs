using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Helpers;
using File = thirdpartynetwork.Core.Models.File;
using Role = thirdpartynetwork.Core.Models.Role;
using Microsoft.AspNetCore.StaticFiles;


namespace thirdpartynetwork
{
    public class DatabaseSeeder(DataContext context, IPasswordHasher passwordHasher, ICDNService cdnService)
    {
        public async Task SeedAsync()
        {
            if (context.Networks.Any()) return;
            await ClearDatabase();

            // Start transaction for safe rollback
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // -------------------
                // 1. Create base data
                // -------------------
                var network = new Network
                {
                    Name = Constants.Networks.Main,
                    Description = "TrojoNetworks' main network",
                    ImageFile = null,
                    RedirectURI = "#",
                    IsSystemProtected = true,
                    IsPublic = true,
                };

                var permissions = new List<Permission>
                {
                    new() { Name = Constants.Permissions.Administrator, Description = "User has ALL permissions and can do ANYTHING.", IsSystemProtected = true },
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
                    new() { Name = Constants.Permissions.ManageFile, Description = "", IsSystemProtected = true }
                };

                var accesses = new List<Access>
                {
                    new() { Name = Constants.Access.AccessUsername, Description = "Allows Network to Access User's Username", IsSystemProtected = true },
                    new() { Name = Constants.Access.AccessFirstname, Description = "Allows Network to Access User's Firstname", IsSystemProtected = true },
                    new() { Name = Constants.Access.AccessLastname, Description = "Allows Network to Access User's Lastname", IsSystemProtected = true },
                    new() { Name = Constants.Access.AccessEmail, Description = "Allows Network to Access User's Email", IsSystemProtected = true },
                    new() { Name = Constants.Access.AccessProfilePicture, Description = "Allows Network to Access User's ProfilePicture", IsSystemProtected = true }
                };

                var adminRole = new Role
                {
                    Name = Constants.Roles.Admin,
                    Description = "Application Administrator",
                    IsSystemProtected = true,
                    Network = network,
                    IsDefault = false,
                };

                adminRole.RolePermissions = new List<RolePermission>
                {
                    new() { Permission = permissions.First(p => p.Name == Constants.Permissions.Administrator), Role = adminRole }
                };

                var userRole = new Role
                {
                    Name = Constants.Roles.User,
                    Description = "Application User",
                    IsSystemProtected = true,
                    Network = network,
                    IsDefault = true,
                };

                var adminUser = new UserProxy
                {
                    Username = "Admin",
                    FirstName = "Admin",
                    LastName = "Noodle",
                    Email = "admin@gmail.com",
                    Password = passwordHasher.HashPassword("LittleNoodleBigNoodle"),
                    ImageFile = null,
                    IsDefault = true,
                    User = new User()
                };

                var adminNetworkUser = new NetworkUser
                {
                    UserProxy = adminUser,
                    Network = network,
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
                    IsRequired = access.Name != Constants.Access.AccessUsername && access.Name != Constants.Access.AccessProfilePicture,
                    IsSystemProtected = true
                }).ToList();

                network.NetworkUsers = new List<NetworkUser> { adminNetworkUser };

                context.Networks.Add(network);
                context.Permissions.AddRange(permissions);

                var networkFile = new File
                {
                    PublicId = Guid.Parse("42870851-0cb1-4bc2-bf3d-80708bcff9a1"),
                    Url = "https://res.cloudinary.com/dm77smcp6/image/upload/v1758664347/01997890-52ad-7de0-8128-5e0b6b885f95/42870851-0cb1-4bc2-bf3d-80708bcff9a1.png",
                    Format = "png",
                    SizeBytes = 634101,
                    Width = 748,
                    Height = 686,
                    MediaType = "image",
                    Name = "networkIcon.png",
                    IsPublic = true,
                    Network = network,
                    Author = adminNetworkUser
                };

                context.Files.Add(networkFile);

                network.ImageFileId = networkFile.Id;

                adminUser.ImageFile = new File
                {
                    PublicId = Guid.Parse("93adb17e-02d5-4fd3-b088-9ca793613a75"),
                    Url = "https://res.cloudinary.com/dm77smcp6/image/upload/v1758664348/01997890-52ad-7de0-8128-5e0b6b885f95/93adb17e-02d5-4fd3-b088-9ca793613a75.png",
                    Format = "image",
                    SizeBytes = 68212,
                    Width = 1024,
                    Height = 1024,
                    MediaType = "image",
                    Name = "adminAvatar.png",
                    IsPublic = true,
                    Network = network,
                    Author = adminNetworkUser
                };

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

            var storedFiles = context.Files.ToList();
            foreach (var file in storedFiles.Where(file => file.Name is not ("networkIcon.png" or "adminAvatar.png")))
            {
                if (!Enum.TryParse<ResourceType>(file.MediaType, true, out var resourceType))
                    throw new InvalidOperationException($"Invalid resource type: {file.MediaType}");

                await cdnService.DeleteAsync(file.Id, resourceType);
            }

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
