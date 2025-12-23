using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using Configuration = thirdpartynetwork.Core.Models.Configuration;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Application.Repositories
{
    public class ConfigurationRepository(DataContext context, IMapper mapper) : BaseRepository(context), IConfigurationRepository
    {
        private readonly DataContext _context = context;
        
        private IQueryable<Configuration> GetQueryable(bool bypass = false)
        {
            var query = _context.Configurations
                .Include(x => x.Author)
                .ThenInclude(x => x.UserProxy)
                .AsQueryable();
            // var currentUser = identityService.GetUserProxy(true);
            
            return query;
        }


        public Configuration Create(Configuration configuration)
        {
            _context.Configurations.Add(configuration);
            SaveAndUpdate(configuration);
            return configuration;
        }

        public void Delete(Configuration configuration)
        {
            _context.Remove(configuration);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Configurations.Any(c => c.Id == id);
        }

        public ICollection<Configuration> GetAll()
        {
            return GetQueryable().Include(c => c.Author).ToList();
        }

        public Configuration? GetById(Guid id)
        {
            return GetQueryable().FirstOrDefault(x => x.Id == id);
        }

        public ICollection<Configuration> GetByNetworkId(Guid networkId)
        {
            return GetQueryable().Where(c => c.Network.Id == networkId).Include(c => c.Author).ToList();
        }

        public Configuration Update(Guid configurationId, UpdateConfigurationDto updateConfiguration)
        {
            var existing = GetById(configurationId);
            if (existing == null) throw new Exception("Configuration not found");

            mapper.Map(updateConfiguration, existing);

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
