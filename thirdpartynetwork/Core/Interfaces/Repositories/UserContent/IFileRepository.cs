﻿using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IFileRepository : IUserContentBaseRepository<File>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<File>> GetByNetworkIdAsync(Guid networkId);
        Task<File?> GetByIdAsync(Guid networkId, Guid id);
        Task<File> CreateAsync(Guid networkId, File file);
        Task<File> UpdateAsync(Guid networkId, Guid fileId, UpdateFileDto updateFile);
        Task DeleteAsync(File file);
    }
}
