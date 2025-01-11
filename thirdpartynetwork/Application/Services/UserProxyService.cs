using AutoMapper;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserProxyService(IUserProxyRepository repository, IMapper mapper) : IUserProxyService
    {
        public ICollection<UserProxy> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<UserProxy> GetByUserId()
        {
            throw new NotImplementedException();
        }

        public UserProxy? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public UserProxy Create(CreateUserProxyDto userProxy)
        {
            var proxy = repository.Create(mapper.Map<UserProxy>(userProxy));
            return mapper.Map<UserProxy>(proxy);
        }

        public UserProxy Update(Guid id, UpdateUserProxyDto userProxy)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
