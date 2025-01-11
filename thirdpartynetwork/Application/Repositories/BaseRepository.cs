using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Repositories;

namespace thirdpartynetwork.Application.Repositories
{
    public abstract class BaseRepository(DataContext context) : IBaseRepository
    {
        public bool Save()
        {
            var saved = context.SaveChanges();
            return saved > 0;
        }
    }
}
