﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Repositories
{
    public class FileRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<File>(context, verifier), IFileRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadFile;

        protected override Task<IQueryable<File>> GetQueryableAsync()
        {
            var query = _context.Files
                .AsQueryable();
            
            return Task.FromResult(query);
        }

        public async Task<File> CreateAsync(Guid networkId, File file)
        {
            await _context.Files.AddAsync(file);
            await SaveAndUpdateAsync(file);
            
            var createdFile = await GetByIdAsync(networkId, file.Id);
            return createdFile ?? throw new InvalidOperationException($"Record for file {file.Id} was created but could not be retrieved.");
        }

        public async Task DeleteAsync(File file)
        {
            if (file.IsSystemProtected) throw new UnauthorizedHttpException("File is system protected");
            
            _context.Remove(file);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Files.AnyAsync(c => c.Id == id);
        }

        public async Task<File?> GetByIdAsync(Guid networkId, Guid id)
        {
            return await (await GetAccessibleQueryableAsync(networkId)).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<File> UpdateAsync(Guid networkId, Guid fileId, UpdateFileDto updateFile)
        {
            var existing = await GetByIdAsync(networkId, fileId);
            if (existing == null) throw new Exception("File not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("File is system protected");

            mapper.Map(updateFile, existing); 

            await SaveAndUpdateAsync(existing);
            return existing;
        }
    }
}
