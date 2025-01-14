using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IRoleRuleRepository : IBaseRepository
{
    bool Exists(Guid roleId, Guid ruleId);
    ICollection<RoleRule> GetAll();
    ICollection<RoleRule> GetByRoleId(Guid roleId);
    ICollection<RoleRule> GetByRuleId(Guid ruleId);
    RoleRule? GetByIds(Guid roleId, Guid ruleId);
    RoleRule Create(RoleRule createRoleRule);
    RoleRule Update(RoleRule updateRoleRule);
    bool Delete(RoleRule roleRule);
}
