using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<CustomPage> CustomPages { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<ContentAccessGrant> ContentAccessGrants { get; set; }
        public DbSet<ContentViewingLink> ContentViewingLinks { get; set; }
        public DbSet<ContentModerationFlag> ContentModerationFlags { get; set; }
        public DbSet<Network> Networks { get; set; }
        public DbSet<NetworkAccess> NetworkAccesses { get; set; }
        public DbSet<NetworkEntitlement> NetworkEntitlements { get; set; }
        public DbSet<NetworkUser> NetworkUsers { get; set; }
        public DbSet<NetworkUserAccess> NetworkUserAccesses { get; set; }
        public DbSet<NetworkUserRole> NetworkUserRoles { get; set; }
        public DbSet<PageBlock> PageBlocks { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Access> Accesses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProxy> UserProxies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Eager Loading
            modelBuilder.Entity<UserProxy>()
                .Navigation(up => up.User)
                .AutoInclude();

            modelBuilder.Entity<User>()
                .Navigation(u => u.UserProxies)
                .AutoInclude();
            
            modelBuilder.Entity<NetworkAccess>()
                .HasKey(np => new { np.NetworkId, np.AccessId });
            modelBuilder.Entity<NetworkAccess>()
                .HasOne(n => n.Network)
                .WithMany(np => np.NetworkAccesses)
                .HasForeignKey(n => n.NetworkId);
            modelBuilder.Entity<NetworkAccess>()
                .HasOne(p => p.Access)
                .WithMany(np => np.NetworkAccesses)
                .HasForeignKey(p => p.AccessId);

            modelBuilder.Entity<NetworkUser>()
                .HasKey(nu => nu.Id);
            modelBuilder.Entity<NetworkUser>()
                .HasOne(n => n.Network)
                .WithMany(nu => nu.NetworkUsers)
                .HasForeignKey(n => n.NetworkId);
            modelBuilder.Entity<NetworkUser>()
                .HasOne(u => u.UserProxy)
                .WithMany(nu => nu.NetworkUsers)
                .HasForeignKey(u => u.UserProxyId);

            modelBuilder.Entity<NetworkUserAccess>()
                .HasKey(nup => new { nup.NetworkUserId, nup.AccessId });
            modelBuilder.Entity<NetworkUserAccess>()
                .HasOne(nu => nu.NetworkUser)
                .WithMany(nup => nup.NetworkUserAccesses)
                .HasForeignKey(nu => nu.NetworkUserId);
            modelBuilder.Entity<NetworkUserAccess>()
                .HasOne(p => p.Access)
                .WithMany(nup => nup.NetworkUserAccesses)
                .HasForeignKey(p => p.AccessId);

            modelBuilder.Entity<NetworkUserRole>()
                .HasKey(nur => new { nur.NetworkUserId, nur.RoleId });
            modelBuilder.Entity<NetworkUserRole>()
                .HasOne(nu => nu.NetworkUser)
                .WithMany(nur => nur.NetworkUserRoles)
                .HasForeignKey(nu => nu.NetworkUserId);
            modelBuilder.Entity<NetworkUserRole>()
                .HasOne(r => r.Role)
                .WithMany(nur => nur.NetworkUserRoles)
                .HasForeignKey(r => r.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rr => new { rr.PermissionId, rr.RoleId });
            modelBuilder.Entity<RolePermission>()
                .HasOne(rr => rr.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rr => rr.RoleId);
            modelBuilder.Entity<RolePermission>()
                .HasOne(rr => rr.Permission)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rr => rr.PermissionId);
            
            modelBuilder.Entity<Network>()
                .HasMany(n => n.Roles)
                .WithOne(r => r.Network)
                .HasForeignKey(r => r.NetworkId);
            
            modelBuilder.Entity<NetworkEntitlement>()
                .HasKey(e => e.NetworkId);

            // One-to-one: Network -> NetworkEntitlement
            modelBuilder.Entity<Network>()
                .HasOne(n => n.Entitlement)
                .WithOne()
                .HasForeignKey<NetworkEntitlement>(e => e.NetworkId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            var entityRole = modelBuilder.Entity<Role>();
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                entityRole
                    .Property(b => b.Entitlements)
                    .HasConversion(
                        v => v.RootElement.GetRawText(),
                        v => JsonDocument.Parse(v, default)
                    );
            }
            entityRole.Property(b => b.Entitlements).HasDefaultValueSql("'{}'");
            
            var entityNetworkUser = modelBuilder.Entity<NetworkUser>();
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                entityNetworkUser
                    .Property(b => b.Entitlements)
                    .HasConversion(
                        v => v.RootElement.GetRawText(),
                        v => JsonDocument.Parse(v, default)
                    );
            }
            entityNetworkUser.Property(b => b.Entitlements).HasDefaultValueSql("'{}'");
            
            // * ----------------------- *
            //         UserContent
            // * ----------------------- *
            
            modelBuilder.Entity<CustomPage>()
                .HasMany(cp => cp.Pages)
                .WithOne(pb => pb.CustomPage)
                .HasForeignKey(pb => pb.CustomPageId)
                .OnDelete(DeleteBehavior.Cascade);

            AuthorToMany<CustomPage>();
            AuthorToMany<PageBlock>();
            AuthorToMany<Configuration>();
            AuthorToMany<Blog>();
            AuthorToMany<File>();
            
            modelBuilder.Entity<CustomPage>()
                .HasOne(p => p.Network)
                .WithMany(n => n.CustomPages)
                .HasForeignKey(m => m.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<PageBlock>()
                .HasOne(p => p.Network)
                .WithMany(n => n.PageBlocks)
                .HasForeignKey(m => m.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Configuration>()
                .HasOne(p => p.Network)
                .WithMany(n => n.Configurations)
                .HasForeignKey(m => m.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Blog>()
                .HasOne(p => p.Network)
                .WithMany(n => n.Blogs)
                .HasForeignKey(m => m.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // FILES
            // One-to-many: Network -> Files
            modelBuilder.Entity<File>()
                .HasOne(f => f.Network)
                .WithMany(n => n.Files)
                .HasForeignKey(f => f.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: Network -> ImageFile
            modelBuilder.Entity<Network>()
                .HasOne(n => n.ImageFile)
                .WithMany()
                .HasForeignKey(f => f.ImageFileId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            
            // One-to-one: UserProxy -> ImageFile
            modelBuilder.Entity<UserProxy>()
                .HasOne(n => n.ImageFile)
                .WithMany()
                .HasForeignKey(f => f.ImageFileId)
                .IsRequired(false);
            
            modelBuilder.Entity<Blog>()
                .HasOne(n => n.CoverImage)
                .WithMany()
                .HasForeignKey(f => f.CoverImageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<File>()
                .HasIndex(f => f.ObjectKey)
                .IsUnique();

            modelBuilder.Entity<PageBlock>()
                .Property(pb => pb.Data)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Configuration>()
                .Property(c => c.Value)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Blog>()
                .Property(b => b.Body)
                .HasColumnType("jsonb");
            
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                modelBuilder.Entity<Blog>()
                    .Property(b => b.Body)
                    .HasConversion(
                        // Serialize: Convert JsonDocument to String for storage
                        doc => doc.RootElement.ToString(),
                        // Deserialize: Convert String back to JsonDocument when reading
                        jsonString => JsonDocument.Parse(jsonString, default)
                    );
                
                
                modelBuilder.Entity<PageBlock>()
                    .Property(pb => pb.Data)
                    .HasConversion(
                        // Serialize: Convert JsonDocument to String for storage
                        doc => doc.RootElement.ToString(),
                        // Deserialize: Convert String back to JsonDocument when reading
                        jsonString => JsonDocument.Parse(jsonString, default)
                    );

                modelBuilder.Entity<Configuration>()
                    .Property(c => c.Value)
                    .HasConversion(
                        // Serialize: Convert JsonDocument to String for storage
                        doc => doc.RootElement.ToString(),
                        // Deserialize: Convert String back to JsonDocument when reading
                        jsonString => JsonDocument.Parse(jsonString, default)
                    );
            }

            // * ----------------------- *
            //      ContentAccessGrant
            // * ----------------------- *

            modelBuilder.Entity<ContentAccessGrant>()
                .HasOne(g => g.GranteeNetwork)
                .WithMany()
                .HasForeignKey(g => g.GranteeNetworkId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContentAccessGrant>()
                .HasOne(g => g.GranteeRole)
                .WithMany()
                .HasForeignKey(g => g.GranteeRoleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContentAccessGrant>()
                .HasOne(g => g.GranteePermission)
                .WithMany()
                .HasForeignKey(g => g.GranteePermissionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContentAccessGrant>()
                .HasOne(g => g.GranteeUserProxy)
                .WithMany()
                .HasForeignKey(g => g.GranteeUserProxyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContentAccessGrant>()
                .HasOne(g => g.GrantedByNetworkUser)
                .WithMany()
                .HasForeignKey(g => g.GrantedByNetworkUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContentAccessGrant>()
                .HasIndex(g => new { g.ContentKind, g.ContentId })
                .HasFilter("\"RevokedAt\" IS NULL");

            modelBuilder.Entity<ContentAccessGrant>()
                .HasIndex(g => new { g.GranteeUserProxyId, g.ContentKind })
                .HasFilter("\"RevokedAt\" IS NULL");

            modelBuilder.Entity<ContentAccessGrant>()
                .HasIndex(g => new { g.GranteeNetworkId, g.ContentKind })
                .HasFilter("\"RevokedAt\" IS NULL");

            // * ----------------------- *
            //      ContentViewingLink
            // * ----------------------- *

            modelBuilder.Entity<ContentViewingLink>()
                .HasOne(l => l.IssuedForUserProxy)
                .WithMany()
                .HasForeignKey(l => l.IssuedForUserProxyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContentViewingLink>()
                .HasOne(l => l.IssuedForNetwork)
                .WithMany()
                .HasForeignKey(l => l.IssuedForNetworkId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContentViewingLink>()
                .HasIndex(l => new { l.ContentKind, l.ContentId });

            modelBuilder.Entity<ContentViewingLink>()
                .HasIndex(l => l.ExpiresAt);

            // * ----------------------- *
            //     ContentModerationFlag
            // * ----------------------- *

            modelBuilder.Entity<ContentModerationFlag>()
                .HasOne(f => f.Network)
                .WithMany()
                .HasForeignKey(f => f.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContentModerationFlag>()
                .HasOne(f => f.FlaggedByNetworkUser)
                .WithMany()
                .HasForeignKey(f => f.FlaggedByNetworkUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ContentModerationFlag>()
                .HasIndex(f => new { f.ContentKind, f.ContentId, f.NetworkId })
                .HasFilter("\"HiddenInNetwork\" = true");

            base.OnModelCreating(modelBuilder);
            return;

            void AuthorToMany<T>() where T : UserContentBaseModel
            {
                modelBuilder.Entity<T>()
                    .HasOne(m => m.AuthorUserProxy)
                    .WithMany()
                    .HasForeignKey(m => m.AuthorUserProxyId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }
    }
}
