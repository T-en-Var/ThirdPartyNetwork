using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class CustomPageRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<CustomPage>(context, verifier), ICustomPageRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadCustomPage;

        protected override IQueryable<CustomPage> GetQueryable()
        {
            return _context.CustomPages.Include(x => x.Pages).AsQueryable();
        }

        public bool Exists(Guid id)
        {
            return _context.CustomPages.Any(c => c.Id == id);
        }

        public CustomPage? GetById(Guid id)
        {
            return GetQueryable().FirstOrDefault(c => c.Id == id);
        }

        public ICollection<CustomPage> GetByNetworkId(Guid networkId)
        {
            return GetAccessibleQueryable(networkId).Where(c => c.Network.Id == networkId).ToList();
        }

        public CustomPage Create(CustomPage customPage)
        {
            _context.CustomPages.Add(customPage);
            SaveAndUpdate(customPage);
            return customPage;
        }

        public CustomPage Update(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage)
        {
            var existing = GetById(customPageId);
            if (existing == null) throw new Exception("CustomPage not found");

            mapper.Map(updateCustomPage, existing);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(CustomPage customPage)
        {
            _context.Remove(customPage);
            _context.SaveChanges();
        }
    }
}