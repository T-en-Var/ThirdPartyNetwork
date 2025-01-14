namespace thirdpartynetwork.Core.Models;

public class Rule
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Guid NetworkId { get; set; }
    public Network Network { get; set; }
    public ICollection<RoleRule> RoleRules { get; set; } = new List<RoleRule>();
    public bool IsSystemProtected { get; set; } = false;
}
