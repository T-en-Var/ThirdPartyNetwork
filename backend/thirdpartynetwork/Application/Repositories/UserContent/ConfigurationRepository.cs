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

        protected override IQueryable<Configuration> GetQueryable()
        {
            return _context.Configurations
                .AsNoTracking()
                .AsQueryable();
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

        public Configuration? GetById(Guid id)
        {
            return GetQueryable().FirstOrDefault(x => x.Id == id);
        }

        public ICollection<Configuration> GetByNetworkId(Guid networkId)
        {
            return GetAccessibleQueryable(networkId).Where(c => c.Network.Id == networkId).ToList();
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
