using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using Configuration = thirdpartynetwork.Core.Models.Configuration;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Repositories
{
    public class ConfigurationRepository(
        DataContext context, 
        IMapper mapper, 
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<Configuration>(context, verifier), IConfigurationRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadConfiguration;

        protected override Task<IQueryable<Configuration>> GetQueryableAsync()
        {
            var query = _context.Configurations
                .AsQueryable();
            
            return Task.FromResult(query);
        }

        public async Task<Configuration> CreateAsync(Configuration configuration)
        {
            await _context.Configurations.AddAsync(configuration);
            await SaveAndUpdateAsync(configuration);
            return configuration;
        }

        public async Task DeleteAsync(Configuration configuration)
        {
            _context.Remove(configuration);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Configurations.AnyAsync(c => c.Id == id);
        }

        public async Task<Configuration?> GetByIdAsync(Guid id)
        {
            return await (await GetQueryableAsync()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<Configuration>> GetByNetworkIdAsync(Guid networkId)
        {
            return await (await GetAccessibleQueryableAsync(networkId))
                .AsNoTracking()
                .Where(c => c.Network.Id == networkId).ToListAsync();
        }

        public async Task<Configuration> UpdateAsync(Guid configurationId, UpdateConfigurationDto updateConfiguration)
        {
            var existing = await GetByIdAsync(configurationId);
            if (existing == null) throw new Exception("Configuration not found");

            mapper.Map(updateConfiguration, existing);

            await SaveAndUpdateAsync(existing);
            return existing;
        }
    }
}
