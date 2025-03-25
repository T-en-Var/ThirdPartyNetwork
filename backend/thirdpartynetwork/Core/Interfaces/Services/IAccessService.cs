using thirdpartynetwork.Application.Dtos.Access;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IAccessService : IBaseService
    {
        ICollection<Access> GetAll();
        Access? GetById(Guid id);
        Access Create(CreateAccessDto createAccess);
        Access Update(Guid id, UpdateAccessDto updateAccess);
        bool DeleteById(Guid id);
    }
}
