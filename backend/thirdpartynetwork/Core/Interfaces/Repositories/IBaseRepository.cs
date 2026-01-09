﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IBaseRepository
    {
        public Task SaveAndUpdateAllAsync<T>(IEnumerable<T> models) where T : BaseModel;
        Task SaveAndUpdateAsync<T>(T model) where T : BaseModel;
    }
}
