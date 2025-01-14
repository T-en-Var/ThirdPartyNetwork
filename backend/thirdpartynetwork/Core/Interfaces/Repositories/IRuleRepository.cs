using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IRuleRepository : IBaseRepository
{
    bool Exists(Guid id);
    ICollection<Rule> GetAll();
    Rule? GetById(Guid id);
    Rule? GetByName(string ruleName);
    Rule Create(Rule rule);
    Rule Update(Rule rule);
    bool Delete(Rule rule);
}
