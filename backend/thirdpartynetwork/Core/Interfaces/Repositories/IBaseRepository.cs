using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IBaseRepository
    {
        public void SaveAndUpdateAll<T>(IEnumerable<T> models) where T : BaseModel;
        void SaveAndUpdate<T>(T model) where T : BaseModel;
    }
}
