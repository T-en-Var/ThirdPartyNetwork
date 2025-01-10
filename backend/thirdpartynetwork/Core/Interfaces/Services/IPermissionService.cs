using thirdpartynetwork.Dtos.Permission;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IPermissionService : IBaseService
    {
        ICollection<PermissionDto> GetAll();
        PermissionDto? GetById(Guid id);
        PermissionDto? Create(CreatePermissionDto permission);
        PermissionDto? Update(Guid id, UpdatePermissionDto permission);
        bool DeleteById(Guid id);
    }
}
