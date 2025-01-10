using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public required DbSet<Content> Contents { get; set; }
        public required DbSet<Network> Networks { get; set; }
        public required DbSet<NetworkPermission> NetworkPermissions { get; set; }
        public required DbSet<NetworkRole> NetworkRoles { get; set; }
        public required DbSet<NetworkUser> NetworkUsers { get; set; }
        public required DbSet<NetworkUserPermission> NetworkUserPermissions { get; set; }
        public required DbSet<NetworkUserRole> NetworkUserRoles { get; set; }
        public required DbSet<Permission> Permissions { get; set; }
        public required DbSet<Role> Roles { get; set; }
        public required DbSet<User> Users { get; set; }
        public required DbSet<UserProxy> UserProxies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkPermission>()
                .HasKey(np => new { np.NetworkId, np.PermissionId });
            modelBuilder.Entity<NetworkPermission>()
                .HasOne(n => n.Network)
                .WithMany(np => np.NetworkPermissions)
                .HasForeignKey(n => n.NetworkId);
            modelBuilder.Entity<NetworkPermission>()
                .HasOne(p => p.Permission)
                .WithMany(np => np.NetworkPermissions)
                .HasForeignKey(p => p.PermissionId);

            modelBuilder.Entity<NetworkRole>()
                .HasKey(nr => new { nr.NetworkId, nr.RoleId });
            modelBuilder.Entity<NetworkRole>()
                .HasOne(n => n.Network)
                .WithMany(nr => nr.NetworkRoles)
                .HasForeignKey(n => n.NetworkId);
            modelBuilder.Entity<NetworkRole>()
                .HasOne(r => r.Role)
                .WithMany(nr => nr.NetworkRoles)
                .HasForeignKey(r => r.RoleId);

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

            modelBuilder.Entity<NetworkUserPermission>()
                .HasKey(nup => new { nup.NetworkUserId, nup.PermissionId });
            modelBuilder.Entity<NetworkUserPermission>()
                .HasOne(nu => nu.NetworkUser)
                .WithMany(nup => nup.NetworkUserPermissions)
                .HasForeignKey(nu => nu.NetworkUserId);
            modelBuilder.Entity<NetworkUserPermission>()
                .HasOne(p => p.Permission)
                .WithMany(nup => nup.NetworkUserPermissions)
                .HasForeignKey(p => p.PermissionId);

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

            //base.OnModelCreating(modelBuilder);
        }
    }
}
