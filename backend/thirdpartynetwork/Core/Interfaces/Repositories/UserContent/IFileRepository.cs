using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IFileRepository : IUserContentBaseRepository<File>
    {
        bool Exists(Guid id);
        ICollection<File> GetByNetworkId(Guid networkId);
        File? GetById(Guid id);
        File Create(File file);
        File Update(Guid fileId, UpdateFileDto updateFile);
        void Delete(File file);
    }
}
