namespace trojoaccounthub.Interfaces.Services
{
    public interface IRoleService : IBaseService
    {
        ICollection<RoleDto> GetAll();
        RoleDto? GetById(Guid id);
        RoleDto? Create(CreateRoleDto role);
        RoleDto? Update(Guid id, UpdateRoleDto role);
        bool DeleteById(Guid id);
    }
}
