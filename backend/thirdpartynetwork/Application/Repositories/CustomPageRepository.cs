using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class CustomPageRepository(DataContext context, IMapper mapper) : BaseRepository(context), ICustomPageRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid id)
        {
            return _context.CustomPages.Any(c => c.Id == id);
        }

        public ICollection<CustomPage> GetAll()
        {
            return [.. _context.CustomPages];
        }

        public CustomPage? GetById(Guid id)
        {
            return _context.CustomPages.Find(id);
        }

        public ICollection<CustomPage> GetByNetworkId(Guid networkId)
        {
            return [.. _context.CustomPages.Where(c => c.Network.Id == networkId)];
        }

        public CustomPage Create(CustomPage customPage)
        {
            _context.CustomPages.Add(customPage);
            SaveAndUpdate(customPage);
            return customPage;
        }

        public CustomPage Update(Guid customPageId, UpdateCustomPageDto updateCustomPage)
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