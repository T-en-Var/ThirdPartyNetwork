﻿using thirdpartynetwork.Application.Dtos.Access;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IAccessService : IBaseService
    {
        Task<ICollection<Access>> GetAllAsync();
        Task<Access?> GetByIdAsync(Guid id);
        Task<Access> CreateAsync(CreateAccessDto createAccess);
        Task<Access> UpdateAsync(Guid id, UpdateAccessDto updateAccess);
        Task DeleteByIdAsync(Guid id);
    }
}
