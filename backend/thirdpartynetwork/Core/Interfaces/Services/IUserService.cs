using thirdpartynetwork.Dtos.User;

namespace thirdpartynetwork.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        ICollection<UserDto> GetAll();
        UserDto? GetById();
        UserDto? Create(CreateUserDto user);
        UserDto? Update(Guid id, UpdateUserDto user);
        bool DeleteById(Guid id);
    }
}
