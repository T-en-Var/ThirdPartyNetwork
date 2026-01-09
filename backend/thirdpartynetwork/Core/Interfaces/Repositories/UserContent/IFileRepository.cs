﻿using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IFileRepository : IUserContentBaseRepository<File>
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<File>> GetByNetworkIdAsync(Guid networkId);
        Task<File?> GetByIdAsync(Guid id);
        Task<File> CreateAsync(File file);
        Task<File> UpdateAsync(Guid fileId, UpdateFileDto updateFile);
        Task DeleteAsync(File file);
    }
}
