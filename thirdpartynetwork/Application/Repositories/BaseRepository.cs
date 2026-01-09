﻿using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public abstract class BaseRepository(DataContext context) : IBaseRepository
    {
        public async Task SaveAndUpdateAllAsync<T>(IEnumerable<T> models) where T : BaseModel
        {
            foreach (var model in models)
            {
                model.UpdatedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }
        
        public async Task SaveAndUpdateAsync<T>(T model) where T : BaseModel
        {
            model.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}
