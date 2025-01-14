using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IRoleRuleService : IBaseService
{
    ICollection<RoleRule> GetAll();
    ICollection<RoleRule> GetByRoleId(Guid networkId, Guid roleId);
    ICollection<RoleRule> GetByRuleId(Guid ruleId);
    RoleRule? GetByIds(Guid networkId, Guid roleId, Guid ruleId);
    RoleRule Create(Guid networkId, Guid roleId, Guid ruleId, CreateRoleRuleDto createRoleRule);
    RoleRule Update(Guid networkId, Guid roleId, Guid ruleId, UpdateRoleRuleDto updateRoleRule);
    bool DeleteByIds(Guid networkId, Guid roleId, Guid ruleId);
}
