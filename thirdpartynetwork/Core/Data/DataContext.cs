using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public required DbSet<CustomPage> CustomPages { get; set; }
        public required DbSet<File> Files { get; set; }
        public required DbSet<Network> Networks { get; set; }
        public required DbSet<NetworkAccess> NetworkAccesses { get; set; }
        public required DbSet<NetworkUser> NetworkUsers { get; set; }
        public required DbSet<NetworkUserAccess> NetworkUserAccesses { get; set; }
        public required DbSet<NetworkUserRole> NetworkUserRoles { get; set; }
        public required DbSet<PageBlock> PageBlocks { get; set; }
        public required DbSet<Access> Accesses { get; set; }
        public required DbSet<Role> Roles { get; set; }
        public required DbSet<RolePermission> RolePermissions { get; set; }
        public required DbSet<Permission> Permissions { get; set; }
        public required DbSet<User> Users { get; set; }
        public required DbSet<UserProxy> UserProxies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .IsRequired(false);

            // One-to-one: UserProxy -> ImageFile
            modelBuilder.Entity<UserProxy>()
                .HasOne(n => n.ImageFile)
                .WithMany()
                .HasForeignKey(f => f.ImageFileId)
                .IsRequired(false);
            
            modelBuilder.Entity<CustomPage>()
                .HasMany(cp => cp.Pages)
                .WithOne()
                .HasForeignKey(pb => pb.CustomPageId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<CustomPage>()
                .HasOne(f => f.Network)
                .WithMany(n => n.CustomPages)
                .HasForeignKey(f => f.NetworkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PageBlock>()
                .Property(pb => pb.Data)
                .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
        }
    }
}
