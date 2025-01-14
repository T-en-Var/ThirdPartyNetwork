namespace thirdpartynetwork.Core.Models;

public class RoleRule
{
    public Guid RoleId { get; set; }
    public Guid RuleId { get; set; }
    public Role Role { get; set; }
    public Rule Rule { get; set; }
}
