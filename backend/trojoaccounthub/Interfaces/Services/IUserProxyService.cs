namespace trojoaccounthub.Interfaces.Services
{
    public interface IUserProxyService : IBaseService
    {
        ICollection<UserProxyDto> GetAll();
        ICollection<UserProxyDto> GetByUserId();
        UserProxyDto? GetById(int id);
        UserProxyDto? Create(CreateUserProxyDto userProxy);
        UserProxyDto? Update(int id, UpdateUserProxyDto userProxy);
        bool DeleteById(int id);
    }
}
