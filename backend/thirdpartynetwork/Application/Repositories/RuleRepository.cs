using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class RuleRepository(DataContext context) : BaseRepository(context), IRuleRepository
{
    private readonly DataContext _context = context;
    
    public bool Exists(Guid id)
    {
        return _context.Rules.Any(r => r.Id == id);
    }

    public ICollection<Rule> GetAll()
    {
        return _context.Rules
            .ToList();
    }

    public Rule? GetById(Guid id)
    {
        return _context.Rules
            .FirstOrDefault(r => r.Id == id);
    }

    public Rule? GetByName(string ruleName)
    {
        return _context.Rules
            .FirstOrDefault(r => r.Name == ruleName);
    }

    public Rule Create(Rule rule)
    {
        _context.Rules.Add(rule);
        Save();
        return rule;
    }

    public Rule Update(Rule rule)
    {
        var existing = GetById(rule.Id);
        if (existing == null) throw new Exception("Rule not found");
            
        if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Rule is system protected");

        _context.Entry(existing).CurrentValues.SetValues(rule);

        Save();
        return existing;
    }

    public bool Delete(Rule rule)
    {
        if (rule.IsSystemProtected) throw new UnauthorizedHttpException("Rule is system protected");

        _context.Remove(rule);
        return Save();
    }
}
