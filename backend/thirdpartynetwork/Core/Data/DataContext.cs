using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public required DbSet<Content> Contents { get; set; }
        public required DbSet<Network> Networks { get; set; }
        public required DbSet<NetworkAccess> NetworkAccesses { get; set; }
        public required DbSet<NetworkUser> NetworkUsers { get; set; }
        public required DbSet<NetworkUserAccess> NetworkUserAccesses { get; set; }
        public required DbSet<NetworkUserRole> NetworkUserRoles { get; set; }
        public required DbSet<Access> Accesses { get; set; }
        public required DbSet<Role> Roles { get; set; }
        public required DbSet<RoleRule> RoleRules { get; set; }
        public required DbSet<Rule> Rules { get; set; }
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

            modelBuilder.Entity<RoleRule>()
                .HasKey(rr => new { rr.RuleId, rr.RoleId });
            modelBuilder.Entity<RoleRule>()
                .HasOne(rr => rr.Role)
                .WithMany(r => r.RoleRules)
                .HasForeignKey(rr => rr.RoleId);
            modelBuilder.Entity<RoleRule>()
                .HasOne(rr => rr.Rule)
                .WithMany(r => r.RoleRules)
                .HasForeignKey(rr => rr.RuleId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
