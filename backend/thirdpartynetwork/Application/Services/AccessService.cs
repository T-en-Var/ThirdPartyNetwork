﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class AccessService(IAccessRepository repository, IMapper mapper) : IAccessService
    {
        public async Task<ICollection<Access>> GetAllAsync()
        {
            var accesses = await repository.GetAllAsync();
            return accesses;
        }

        public async Task<Access?> GetByIdAsync(Guid id)
        {
            var access = await repository.GetByIdAsync(id);
            return access;
        }

        public async Task<Access> CreateAsync(CreateAccessDto createAccess)
        {
            var access = mapper.Map<Access>(createAccess);
            access = await repository.CreateAsync(access);
            return access;
        }

        public async Task<Access> UpdateAsync(Guid id, UpdateAccessDto updateAccess)
        {
            if (!await repository.ExistsAsync(id)) throw new NotFoundHttpException();
            
            var access = mapper.Map<Access>(updateAccess);
            access.Id = id;
            
            access = await repository.UpdateAsync(access);
            return access;
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var access = await GetByIdAsync(id);
            if (access != null) await repository.DeleteAsync(access);
        }
    }
}
