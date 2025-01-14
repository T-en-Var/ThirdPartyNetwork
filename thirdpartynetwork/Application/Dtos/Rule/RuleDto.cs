using thirdpartynetwork.Application.Dtos.NetworkAccess;
using thirdpartynetwork.Application.Dtos.NetworkUserAccess;

namespace thirdpartynetwork.Application.Dtos.Rule
{
    public class RuleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public ICollection<RoleRuleDto> RoleRules { get; set; } = [];
    }
}
