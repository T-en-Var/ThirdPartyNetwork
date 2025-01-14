namespace thirdpartynetwork.Application.Dtos.RoleRule;

public class RoleRuleDto
{
    public Guid RoleId { get; set; }
    public Guid RuleId { get; set; }
    public RoleDto Role { get; set; }
    public RuleDto Rule { get; set; }
}