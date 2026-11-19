﻿using thirdpartynetwork.Application.Dtos.User;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        Task<ICollection<User>> GetAllAsync();
        Task<ICollection<UserMetricsDto>> GetAllWithMetricsAsync();
        Task<UserMetricsDto> GetByIdWithMetricsAsync(Guid userId);
        Task<User?> GetByIdAsync(Guid userId);
        Task<User> CreateAsync(CreateUserDto createUser);
        Task<User> UpdateAsync(Guid userId, UpdateUserDto updateUser);
        Task DeleteByIdAsync(Guid userId);
    }
}
