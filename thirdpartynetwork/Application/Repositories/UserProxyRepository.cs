using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class UserProxyRepository(
        DataContext context,
        IIdentityVerifier identityVerifier,
        IIdentityService identityService) : BaseRepository(context), IUserProxyRepository
    {
        private readonly DataContext _context = context;

        private IQueryable<UserProxy> GetQueryable()
        {
            var query = _context.UserProxies.AsQueryable();

            query = query
                .AsNoTracking()
                .Include(x => x.User)
                .ThenInclude(x => x.UserProxies)
                .Include(x => x.NetworkUsers)
                .ThenInclude(x => x.Network);

            return query;
        }

        public bool Exists(Guid userProxyId)
        {
            return _context.UserProxies.Any(up => up.Id == userProxyId);
        }

        public ICollection<UserProxy> GetAll()
        {
            return [.. GetQueryable()];
        }

        public UserProxy? GetById(Guid userProxyId)
        {
            var query = GetQueryable();
            var currentUser = identityService.GetUserProxy(true);

            if (identityVerifier.IsUserProxyOwner(currentUser.Id, userProxyId))
            {
                query = query
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Files)
                    
                    .Include(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Files)
                    
                    .Include(up => up.ImageFile)
                    
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Network)
                    .ThenInclude(nu => nu.ImageFile)
                    
                    .Include(up => up.User)
                    .ThenInclude(u => u.UserProxies)
                    .ThenInclude(up => up.ImageFile)
                    
                    .Include(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.NetworkUserAccesses)
                    .ThenInclude(nua => nua.Access)
                    
                    .Include(up => up.NetworkUsers)
                    .ThenInclude(nu => nu.Network)
                    .ThenInclude(nu => nu.ImageFile);
            }

            return query.FirstOrDefault(up => up.Id == userProxyId);
        }

        public UserProxy? GetByUsername(string username)
        {
            var query = GetQueryable();

            return query.FirstOrDefault(up => up.Username == username);
        }

        public ICollection<UserProxy> GetByUserId(Guid userId)
        {
            return
            [
                .. GetQueryable()
                    .Where(up => up.User.Id == userId)
            ];
        }

        public UserProxy Create(UserProxy userProxy)
        {
            _context.UserProxies.Add(userProxy);
            SaveAndUpdate(userProxy);
            return userProxy;
        }

        public void SetDefaultUserProxy(Guid userProxyId)
        {
            var userProxy = GetById(userProxyId);
            if (userProxy == null) throw new Exception("UserProxy not found");
            
            if (userProxy.IsDefault) return;
            
            foreach (var userUserProxy in userProxy.User.UserProxies)
            {
                userUserProxy.IsDefault = false;
            }
            
            userProxy.IsDefault = true;
            // don't save, assume this is part of another process
            // assume that process calls _context.SaveChanges();
        }

        public UserProxy Update(UserProxy userProxy)
        {
            var existing = GetById(userProxy.Id);
            if (existing == null) throw new Exception("UserProxy not found");

            _context.Entry(existing).CurrentValues.SetValues(userProxy);

            SaveAndUpdate(existing);
            return existing;
        }

        public IQueryable<UserProxy> GetByIdQueryable(Guid id)
        {
            return GetQueryable()
                .Where(u => u.Id == id);
        }

        public void Delete(UserProxy userProxy)
        {
            _context.Remove(userProxy);
            _context.SaveChanges();
        }
    }
}