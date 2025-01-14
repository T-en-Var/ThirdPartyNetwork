using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class RuleService(IRuleRepository repository, INetworkService networkService, IMapper mapper) : IRuleService
{
    public ICollection<Rule> GetAll()
    {
        var rules = repository.GetAll();
        return rules;
    }

    public Rule? GetById(Guid ruleId)
    {
        var rule = repository.GetById(ruleId);
        return rule;
    }

    public Rule Create(CreateRuleDto createRule)
    {
        var rule = mapper.Map<Rule>(createRule);

        rule = repository.Create(rule);
        return rule;
    }

    public Rule Update(Guid ruleId, UpdateRuleDto updateRule)
    {
        if (!repository.Exists(ruleId)) throw new NotFoundHttpException("Rule not found.");
        
        var rule = mapper.Map<Rule>(updateRule);
        rule.Id = ruleId;
        
        rule = repository.Update(rule);
        return rule;
    }

    public bool DeleteById(Guid ruleId)
    {
        var rule = repository.GetById(ruleId);
        return rule != null && repository.Delete(rule);
    }
}
