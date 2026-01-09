﻿using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IFileService : IUserContentBaseService<File>
    {
        Task<ICollection<File>> GetByNetworkIdAsync(Guid networkId);
        Task<File?> GetByIdAsync(Guid networkId, Guid fileId);
        // File Create(Guid networkId, CreateFileDto createFile);
        Task<File> UploadFileAsync(Guid networkId, IFormFile file);
        Task<File> UpdateAsync(Guid networkId, Guid fileId, UpdateFileDto updateFile);
        Task DeleteByIdAsync(Guid networkId, Guid fileId);
    }
}
