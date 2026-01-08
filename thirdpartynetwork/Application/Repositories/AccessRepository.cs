using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class AccessRepository(DataContext context) : BaseRepository(context), IAccessRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<Access> GetQueryable()
        {
            var query = _context.Accesses
                .AsQueryable();
            
            // I'll skip this for now.
            
            return query;
        }

        public bool Exists(Guid id)
        {
            return _context.Accesses.Any(p => p.Id == id);
        }

        public ICollection<Access> GetAll()
        {
            return [.. GetQueryable().AsNoTracking()];
        }

        public Access? GetById(Guid id)
        {
            return GetQueryable().FirstOrDefault(x => x.Id == id);
        }

        public Access Create(Access access)
        {
            _context.Accesses.Add(access);
            SaveAndUpdate(access);
            return access;
        }

        public Access Update(Access access)
        {
            var existing = GetById(access.Id);
            if (existing == null) throw new Exception("Access not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Access is system protected");

            _context.Entry(existing).CurrentValues.SetValues(access);

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(Access access)
        {
            if (access.IsSystemProtected) throw new UnauthorizedHttpException("Access is system protected");

            _context.Remove(access);
            _context.SaveChanges();
        }
    }
}
