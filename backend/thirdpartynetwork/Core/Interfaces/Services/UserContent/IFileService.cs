using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IFileService : IBaseService
    {
        ICollection<File> GetAll();
        ICollection<File> GetByNetworkId(Guid networkId);
        File? GetById(Guid networkId, Guid fileId);
        // File Create(Guid networkId, CreateFileDto createFile);
        Task<File> UploadFileAsync(Guid networkId, IFormFile file);
        File Update(Guid networkId, Guid fileId, UpdateFileDto updateFile);
        Task DeleteByIdAsync(Guid networkId, Guid fileId);
    }
}
