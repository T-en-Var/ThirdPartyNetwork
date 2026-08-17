namespace thirdpartynetwork.Core.Enum;

/// <summary>PLAN.md §2.4: who a ContentAccessGrant row targets.</summary>
public enum GranteeType
{
    Network = 0,
    NetworkRole = 1,
    NetworkPermission = 2,
    User = 3,
}
