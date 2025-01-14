using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class RoleRuleRepository(DataContext context) : BaseRepository(context), IRoleRuleRepository
{
    private readonly DataContext _context = context;
    
    public bool Exists(Guid roleId, Guid ruleId)
    {
        return _context.RoleRules.Any(r => r.RoleId == roleId && r.RuleId == ruleId);
    }

    public ICollection<RoleRule> GetAll()
    {
        return _context.RoleRules
            .ToList();
    }

    public ICollection<RoleRule> GetByRoleId(Guid roleId)
    {
        return _context.RoleRules
            .Where(r => r.RoleId == roleId)
            .ToList();
    }

    public ICollection<RoleRule> GetByRuleId(Guid ruleId)
    {
        return _context.RoleRules
            .Where(r => r.RuleId == ruleId)
            .ToList();
    }

    public RoleRule? GetByIds(Guid roleId, Guid ruleId)
    {
        return _context.RoleRules
            .FirstOrDefault(r => r.RoleId == roleId && r.RuleId == ruleId);
    }

    public RoleRule Create(RoleRule roleRule)
    {
        _context.RoleRules.Add(roleRule);
        Save();
        return roleRule;
    }

    public RoleRule Update(RoleRule roleRule)
    {
        var existing = GetByIds(roleRule.RoleId, roleRule.RuleId);
        if (existing == null) throw new Exception("RoleRule not found");
            
        _context.Entry(existing).CurrentValues.SetValues(roleRule);

        Save();
        return existing;
    }

    public bool Delete(RoleRule roleRule)
    {
        _context.Remove(roleRule);
        return Save();
    }
}