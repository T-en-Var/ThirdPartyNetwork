namespace trojoaccounthub.Interfaces.Services
{
    public interface IPermissionService : IBaseService
    {
        ICollection<PermissionDto> GetAll();
        PermissionDto? GetById(int id);
        PermissionDto? Create(CreatePermissionDto permission);
        PermissionDto? Update(int id, UpdatePermissionDto permission);
        bool DeleteById(int id);
    }
}
