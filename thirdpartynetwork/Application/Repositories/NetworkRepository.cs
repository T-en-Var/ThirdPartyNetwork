using Microsoft.EntityFrameworkCore;
using Npgsql;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class NetworkRepository(
        DataContext context,
        IIdentityService identityService,
        IIdentityVerifier identityVerifier) : BaseRepository(context), INetworkRepository
    {
        private readonly DataContext _context = context;

        private async Task<IQueryable<Network>> GetQueryableAsync(Guid id, bool bypass = false)
        {
            var query = _context.Networks
                .Include(x => x.ImageFile)
                .Include(x => x.NetworkUsers)
                .Include(x => x.Roles)
                .Include(x => x.NetworkAccesses)
                .ThenInclude(x => x.Access)
                .AsQueryable();

            var currentUser = await identityService.GetUserProxyAsync(true);

            var allowFeatureFlag = bypass ||
                                   await identityVerifier.IsAllowedAsync(id, currentUser.Id,
                                       Constants.Permissions.ManageNetwork);

            if (allowFeatureFlag)
            {
                query = query.Include(x => x.FeatureFlag);
            }

            return query;
        }

        private async Task<IQueryable<Network>> GetQueryableReduced(bool bypass = false)
        {
            var query = _context.Networks
                .Include(x => x.ImageFile)
                .Include(x => x.NetworkUsers)
                .Include(x => x.Roles)
                .Include(x => x.FeatureFlag)
                .AsQueryable();

            var currentUser = await identityService.GetUserProxyAsync(true);

            var allowFeatureFlag = bypass ||
                                   await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(currentUser.Id,
                                       Constants.Permissions.ManageNetwork);
            Console.WriteLine($"allowFeatureFlag: {allowFeatureFlag}");

            if (allowFeatureFlag)
            {
                query = query.Include(x => x.FeatureFlag);
            }

            return query;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Networks.AnyAsync(n => n.Id == id);
        }

        public async Task<ICollection<Network>> GetAllAsync()
        {
            return await (await GetQueryableReduced()).ToListAsync();
        }

        public async Task<ICollection<NetworkMetricsDto>> GetWithMetricsAsync(Guid? networkId = null)
        {
            // fuck it we ball, time to stop fighting LINQ
            // embrace raw SQL
            // embrace the pain of updating it everytime with no reminders this exists after updating the model
            // poggers
            const string sql = """
                SELECT 
                   n."Id" AS "NetworkId",
                   COALESCE(f.FileCount, 0)::int AS "FileCount",
                   COALESCE(f.FileStorage, 0)::bigint AS "FileStorage",
                   COALESCE(b.BlogCount, 0)::int AS "BlogCount",
                   COALESCE(c.ConfigurationCount, 0)::int AS "ConfigurationCount",
                   COALESCE(cp.CustomPageCount, 0)::int AS "CustomPageCount",
                   COALESCE(pb.CustomPageBlockCount, 0)::int AS "CustomPageBlockCount",
                   COALESCE(pb.CustomPageBlockSize, 0)::int AS "CustomPageBlockSize"
                FROM public."Networks" AS n
                LEFT JOIN (
                   SELECT "NetworkId", COUNT(*) AS FileCount, SUM(CAST("SizeBytes" AS BIGINT)) AS FileStorage 
                   FROM public."Files" GROUP BY "NetworkId"
                ) f ON n."Id" = f."NetworkId"
                LEFT JOIN (
                    SELECT "NetworkId", COUNT(*) AS BlogCount FROM public."Blogs" GROUP BY "NetworkId"
                ) b ON n."Id" = b."NetworkId"
                LEFT JOIN (
                    SELECT "NetworkId", COUNT(*) AS ConfigurationCount FROM public."Configurations" GROUP BY "NetworkId"
                ) c ON n."Id" = c."NetworkId"
                LEFT JOIN (
                    SELECT "NetworkId", COUNT(*) AS CustomPageCount FROM public."CustomPages" GROUP BY "NetworkId"
                ) cp ON n."Id" = cp."NetworkId"
                LEFT JOIN (
                    SELECT "NetworkId", 
                        COUNT(*) AS CustomPageBlockCount,
                        SUM(CAST(LENGTH("Data"::text) AS BIGINT)) AS CustomPageBlockSize 
                    FROM public."PageBlocks" GROUP BY "NetworkId"
                ) pb ON n."Id" = pb."NetworkId"
                WHERE (CAST(@networkId AS uuid) IS NULL OR n."Id" = @networkId)
            """;
            
            var parameter = new NpgsqlParameter("networkId", (object)networkId ?? DBNull.Value);
            
            return await _context.Database
                .SqlQueryRaw<NetworkMetricsDto>(sql, parameter)
                .ToListAsync();
        }

        public async Task<Network?> GetByIdAsync(Guid id, bool bypass = false)
        {
            return await (await GetQueryableAsync(id, bypass)).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Network?> GetDetailsByIdAsync(Guid id)
        {
            var network = await _context.Networks
                .Include(x => x.NetworkAccesses)
                .ThenInclude(x => x.Access)
                .Include(x => x.NetworkUsers)
                // .ThenInclude(n => n.NetworkUserAccesses)
                .Include(n => n.ImageFile)
                
                .FirstOrDefaultAsync(n => n.Id == id);
            return network;
        }

        public async Task<Network> GetMainNetworkAsync()
        {
            var mainNetwork = await _context.Networks
                .FirstOrDefaultAsync(n => n.Name == Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");

            return mainNetwork;
        }

        public async Task<Network> GetMainNetworkDetailsAsync()
        {
            var mainNetwork = await _context.Networks.Include(x => x.FeatureFlag)
                .FirstOrDefaultAsync(n => n.Name == Constants.Networks.Main);
            if (mainNetwork == null) throw new Exception("Main network not found, that is NOT good.");

            return mainNetwork;
        }

        public async Task<Network?> GetByNameAsync(string name)
        {
            var mainNetwork = await GetMainNetworkAsync();
            return await (await GetQueryableAsync(mainNetwork.Id)).FirstOrDefaultAsync(n => n.Name == name);
        }

        public IQueryable<Network> GetByNameQueryable(string name)
        {
            return _context.Networks
                .Where(n => n.Name == name);
        }

        public IQueryable<Network> GetByIdQueryable(Guid id)
        {
            return _context.Networks
                .Where(n => n.Id == id);
        }

        public async Task<Network?> GetByIdFeatureFlagAsync(Guid id)
        {
            return await _context.Networks
                .Include(n => n.FeatureFlag)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Network> CreateAsync(Network network)
        {
            await _context.Networks.AddAsync(network);
            await SaveAndUpdateAsync(network);
            return network;
        }

        public async Task<Network> UpdateAsync(Network network)
        {
            var existing = await GetByIdAsync(network.Id);
            if (existing == null) throw new Exception("Network not found");

            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");

            _context.Entry(existing).CurrentValues.SetValues(network);

            await SaveAndUpdateAsync(existing);
            return existing;
        }

        public async Task DeleteAsync(Network network)
        {
            if (network.IsSystemProtected) throw new UnauthorizedHttpException("Network is system protected");

            _context.Remove(network);
            await _context.SaveChangesAsync();
        }
    }
}