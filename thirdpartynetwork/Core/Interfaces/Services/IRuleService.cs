using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IRuleService : IBaseService
{
    ICollection<Rule> GetAll();
    Rule? GetById(Guid ruleId);
    Rule Create(CreateRuleDto createRule);
    Rule Update(Guid ruleId, UpdateRuleDto updateRule);
    bool DeleteById(Guid ruleId);
}
