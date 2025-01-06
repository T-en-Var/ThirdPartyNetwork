namespace trojoaccounthub.Interfaces.Services
{
    public interface IRoleService : IBaseService
    {
        ICollection<RoleDto> GetAll();
        RoleDto? GetById(int id);
        RoleDto? Create(CreateRoleDto role);
        RoleDto? Update(int id, UpdateRoleDto role);
        bool DeleteById(int id);
    }
}
