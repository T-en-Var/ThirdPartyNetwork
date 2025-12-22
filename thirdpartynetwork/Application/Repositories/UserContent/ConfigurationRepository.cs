using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using Configuration = thirdpartynetwork.Core.Models.Configuration;

namespace thirdpartynetwork.Application.Repositories
{
    public class ConfigurationRepository(DataContext context, IMapper mapper) : BaseRepository(context), IConfigurationRepository
    {
        private readonly DataContext _context = context;

        public Configuration Create(Configuration configuration)
        {
            _context.Configurations.Add(configuration);
            SaveAndUpdate(configuration);
            return configuration;
        }

        public void Delete(Configuration configuration)
        {
            if (configuration.IsSystemProtected) throw new UnauthorizedHttpException("Configuration is system protected");

            _context.Remove(configuration);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Configurations.Any(c => c.Id == id);
        }

        public ICollection<Configuration> GetAll()
        {
            return _context.Configurations.Include(c => c.Author).ToList();
        }

        public Configuration? GetById(Guid id)
        {
            return _context.Configurations.Find(id);
        }

        public ICollection<Configuration> GetByNetworkId(Guid networkId)
        {
            return _context.Configurations.Where(c => c.Network.Id == networkId).Include(c => c.Author).ToList();
        }

        public Configuration Update(Guid configurationId, UpdateConfigurationDto updateConfiguration)
        {
            var existing = GetById(configurationId);
            if (existing == null) throw new Exception("Configuration not found");

            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Configuration is system protected");

            mapper.Map(updateConfiguration, existing);

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
