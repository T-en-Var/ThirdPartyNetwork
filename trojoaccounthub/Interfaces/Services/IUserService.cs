namespace trojoaccounthub.Interfaces.Services
{
    public interface IUserService : IBaseService
    {
        ICollection<UserDto> GetAll();
        UserDto? GetById();
        UserDto? Create(CreateUserDto user);
        UserDto? Update(int id, UpdateUserDto user);
        bool DeleteById(int id);
    }
}
