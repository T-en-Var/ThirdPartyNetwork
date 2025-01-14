using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class RoleRuleService(IRoleRuleRepository repository, IRoleService roleService, IRuleService ruleService, IMapper mapper) : IRoleRuleService
{
    public ICollection<RoleRule> GetAll()
    {
        var rules = repository.GetAll();
        return rules;
    }

    public ICollection<RoleRule> GetByRoleId(Guid networkId, Guid roleId)
    {
        var rules = repository.GetByRoleId(roleId);
        return rules;
    }

    public ICollection<RoleRule> GetByRuleId(Guid ruleId)
    {
        var rules = repository.GetByRuleId(ruleId);
        return rules;
    }

    public RoleRule? GetByIds(Guid networkId, Guid roleId, Guid ruleId)
    {
        var rule = repository.GetByIds(roleId, ruleId);
        return rule;
    }

    public RoleRule Create(Guid networkId, Guid roleId, Guid ruleId, CreateRoleRuleDto createRoleRule)
    {
        if (roleService.GetById(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (ruleService.GetById(ruleId) != null) throw new NotFoundHttpException("Rule not found.");
        
        if (repository.Exists(roleId, ruleId)) throw new ConflictHttpException("Already exists.");
        
        var roleRule = mapper.Map<RoleRule>(createRoleRule);
        roleRule.RoleId = roleId;
        roleRule.RuleId = ruleId;
        
        roleRule = repository.Create(roleRule);
        return roleRule;
    }

    public RoleRule Update(Guid networkId, Guid roleId, Guid ruleId, UpdateRoleRuleDto updateRoleRule)
    {
        if (roleService.GetById(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (ruleService.GetById(ruleId) == null) throw new NotFoundHttpException("Rule not found.");
        
        if (!repository.Exists(roleId, ruleId)) throw new NotFoundHttpException();
        
        var roleRule = mapper.Map<RoleRule>(updateRoleRule);
        roleRule.RoleId = roleId;
        roleRule.RuleId = ruleId;
        
        roleRule = repository.Update(roleRule);
        return roleRule;
    }

    public bool DeleteByIds(Guid networkId, Guid roleId, Guid ruleId)
    {
        var roleRule = repository.GetByIds(roleId, ruleId);
        return roleRule != null && repository.Delete(roleRule);
    }
}
