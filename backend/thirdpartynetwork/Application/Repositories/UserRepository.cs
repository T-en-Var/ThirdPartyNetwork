using Microsoft.EntityFrameworkCore;
using Npgsql;
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
                .ThenInclude(x => x.Entitlement)
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
            return await GetQueryable().AsNoTracking().ToListAsync();
        }

        public async Task<ICollection<UserMetricsDto>> GetWithMetricsAsync(Guid? userId = null, Guid? networkId = null)
        {
            // fuck it we ball, time to stop fighting LINQ
            // embrace raw SQL
            // embrace the pain of updating it everytime with no reminders this exists after updating the model
            // poggers
            const string sql =
                """
                    WITH UserAuthors AS (
                        SELECT 
                            nu."Id" AS "AuthorId",
                            up."UserId" AS "UserId"
                        FROM public."NetworkUsers" AS nu
                        JOIN public."UserProxies" AS up ON nu."UserProxyId" = up."Id"
                        WHERE (CAST(@userId AS uuid) IS NULL OR up."UserId" = @userId)
                        AND (CAST(@networkId AS uuid) IS NULL OR nu."NetworkId" = @networkId)
                    )
                    SELECT 
                        ua."UserId" AS "UserId",
                        COALESCE(SUM(f.FileCount), 0)::int AS "FileCount",
                        COALESCE(SUM(f.FileStorage), 0)::bigint AS "FileStorage",
                        COALESCE(SUM(b.BlogCount), 0)::int AS "BlogCount",
                        COALESCE(SUM(c.ConfigurationCount), 0)::int AS "ConfigurationCount",
                        COALESCE(SUM(cp.CustomPageCount), 0)::int AS "CustomPageCount",
                        COALESCE(SUM(pb.CustomPageBlockCount), 0)::int AS "CustomPageBlockCount",
                        COALESCE(SUM(pb.CustomPageBlockSize), 0)::int AS "CustomPageBlockSize"
                    FROM UserAuthors AS ua
                    LEFT JOIN (
                        SELECT "AuthorId", COUNT(*) AS FileCount, SUM(CAST("SizeBytes" AS BIGINT)) AS FileStorage 
                        FROM public."Files" 
                        WHERE "AuthorId" IN (SELECT "AuthorId" FROM UserAuthors)
                        GROUP BY "AuthorId"
                    ) f ON ua."AuthorId" = f."AuthorId"
                    LEFT JOIN (
                        SELECT "AuthorId", COUNT(*) AS BlogCount 
                        FROM public."Blogs" 
                        WHERE "AuthorId" IN (SELECT "AuthorId" FROM UserAuthors)
                        GROUP BY "AuthorId"
                    ) b ON ua."AuthorId" = b."AuthorId"
                    LEFT JOIN (
                        SELECT "AuthorId", COUNT(*) AS ConfigurationCount 
                        FROM public."Configurations" 
                        WHERE "AuthorId" IN (SELECT "AuthorId" FROM UserAuthors)
                        GROUP BY "AuthorId"
                    ) c ON ua."AuthorId" = c."AuthorId"
                    LEFT JOIN (
                        SELECT "AuthorId", COUNT(*) AS CustomPageCount 
                        FROM public."CustomPages" 
                        WHERE "AuthorId" IN (SELECT "AuthorId" FROM UserAuthors)
                        GROUP BY "AuthorId"
                    ) cp ON ua."AuthorId" = cp."AuthorId"
                    LEFT JOIN (
                        SELECT "AuthorId", 
                            COUNT(*) AS CustomPageBlockCount,
                            SUM(CAST(LENGTH("Data"::text) AS BIGINT)) AS CustomPageBlockSize 
                        FROM public."PageBlocks" 
                        WHERE "AuthorId" IN (SELECT "AuthorId" FROM UserAuthors)
                        GROUP BY "AuthorId"
                    ) pb ON ua."AuthorId" = pb."AuthorId"
                    GROUP BY ua."UserId";
                """;

            var parameter = new NpgsqlParameter("userId", (object)userId ?? DBNull.Value);
            var parameter2 = new NpgsqlParameter("networkId", (object)networkId ?? DBNull.Value);

            return await _context.Database
                .SqlQueryRaw<UserMetricsDto>(sql, parameter, parameter2)
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