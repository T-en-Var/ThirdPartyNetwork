using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;

namespace thirdpartynetwork.Repositories
{
    public abstract class BaseRepository : IBaseRepository
    {
        private readonly DataContext _context;
        public BaseRepository(DataContext context)
        {
            _context = context;
        }

        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0;
        }
    }
}
