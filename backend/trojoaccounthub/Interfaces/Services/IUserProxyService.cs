namespace trojoaccounthub.Interfaces.Services
{
    public interface IUserProxyService : IBaseService
    {
        ICollection<UserProxyDto> GetAll();
        ICollection<UserProxyDto> GetByUserId();
        UserProxyDto? GetById(Guid id);
        UserProxyDto? Create(CreateUserProxyDto userProxy);
        UserProxyDto? Update(Guid id, UpdateUserProxyDto userProxy);
        bool DeleteById(Guid id);
    }
}
