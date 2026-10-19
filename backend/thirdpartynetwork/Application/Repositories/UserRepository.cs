using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserRepository(DataContext context) : BaseRepository(context), IUserRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<User> GetQueryable()
        {
            return _context.Users
                .Include(u => u.UserProxies)
                .ThenInclude(x => x.NetworkUsers)
                .ThenInclude(x => x.NetworkUserAccesses)
                .ThenInclude(x => x.Access)
                .Include(u => u.UserProxies)
                .ThenInclude(x => x.NetworkUsers)
                .ThenInclude(x => x.Network)
                .ThenInclude(x => x.ImageFile)
                .Include(u => u.UserProxies)
                .ThenInclude(x => x.NetworkUsers)
                .ThenInclude(x => x.NetworkUserRoles)
                .Include(u => u.UserProxies)
                .ThenInclude(x => x.NetworkUsers)
                .ThenInclude(x => x.Network)
                .ThenInclude(x => x.FeatureFlag)
                .Include(u => u.UserProxies)
                .ThenInclude(x => x.ImageFile)
                .AsQueryable();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<ICollection<User>> GetAllAsync()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task<ICollection<UserMetricsDto>> GetWithMetricsAsync(Guid? userId = null, Guid? networkId = null)
        {
            // Author-owned content is now keyed by AuthorUserProxyId -> UserProxies, not
            // NetworkUsers. Each content table is correlated per proxy via a scalar subquery
            // (EF translates this more reliably than chained GroupBy+LeftJoin), then rolled
            // up to the owning user, since a user can have more than one proxy.
            var perProxy = _context.UserProxies
                .Where(p => userId == null || p.User.Id == userId)
                .Select(p => new
                {
                    UserId = p.User.Id,
                    FileCount = _context.Files.Count(f =>
                        f.AuthorUserProxyId == p.Id && (networkId == null || f.NetworkId == networkId)),
                    FileStorage = _context.Files
                        .Where(f => f.AuthorUserProxyId == p.Id && (networkId == null || f.NetworkId == networkId))
                        .Sum(f => (long?)f.SizeBytes) ?? 0,
                    BlogCount = _context.Blogs.Count(b =>
                        b.AuthorUserProxyId == p.Id && (networkId == null || b.NetworkId == networkId)),
                    ConfigurationCount = _context.Configurations.Count(c =>
                        c.AuthorUserProxyId == p.Id && (networkId == null || c.NetworkId == networkId)),
                    CustomPageCount = _context.CustomPages.Count(cp =>
                        cp.AuthorUserProxyId == p.Id && (networkId == null || cp.NetworkId == networkId)),
                    CustomPageBlockCount = _context.PageBlocks.Count(pb =>
                        pb.AuthorUserProxyId == p.Id && (networkId == null || pb.NetworkId == networkId)),
                });

            return await perProxy
                .GroupBy(x => x.UserId)
                .Select(g => new UserMetricsDto
                {
                    UserId = g.Key,
                    FileCount = g.Sum(x => x.FileCount),
                    FileStorage = g.Sum(x => x.FileStorage),
                    BlogCount = g.Sum(x => x.BlogCount),
                    ConfigurationCount = g.Sum(x => x.ConfigurationCount),
                    CustomPageCount = g.Sum(x => x.CustomPageCount),
                    CustomPageBlockCount = g.Sum(x => x.CustomPageBlockCount),
                })
                .Where(x => x.FileCount > 0 || x.BlogCount > 0 || x.ConfigurationCount > 0
                    || x.CustomPageCount > 0 || x.CustomPageBlockCount > 0)
                .ToListAsync();
        }


        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await SaveAndUpdateAsync(user);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var existing = await GetByIdAsync(user.Id);
            if (existing == null) throw new Exception("User not found");

            _context.Entry(existing).CurrentValues.SetValues(user);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(User user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}