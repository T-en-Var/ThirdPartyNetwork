using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<CustomPage> CustomPages { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Network> Networks { get; set; }
        public DbSet<NetworkAccess> NetworkAccesses { get; set; }
        public DbSet<NetworkUser> NetworkUsers { get; set; }
        public DbSet<NetworkUserAccess> NetworkUserAccesses { get; set; }
        public DbSet<NetworkUserRole> NetworkUserRoles { get; set; }
        public DbSet<PageBlock> PageBlocks { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<Access> Accesses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProxy> UserProxies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
            AuthorToMany<File>();

            NetworkToMany<CustomPage>();
            NetworkToMany<Configuration>();
            
            // FILES
            // One-to-many: Network -> Files
            modelBuilder.Entity<File>()
                .HasOne(f => f.Network)
                .WithMany()
                .HasForeignKey(f => f.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<PageBlock>()
                .Property(pb => pb.Data)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Configuration>()
                .Property(c => c.Value)
                .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
            return;

            void AuthorToMany<T>() where T : UserContentBaseModel
            {
                modelBuilder.Entity<T>()
                    .HasOne(m => m.Author)
                    .WithMany()
                    .HasForeignKey(m => m.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
            
            void NetworkToMany<T>() where T : UserContentBaseModel
            {
                modelBuilder.Entity<T>()
                    .HasOne(m => m.Network)
                    .WithMany()
                    .HasForeignKey(m => m.NetworkId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
}
