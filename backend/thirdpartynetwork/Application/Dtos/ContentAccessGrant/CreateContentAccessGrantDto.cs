using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.ContentAccessGrant;

public class CreateContentAccessGrantDto
{
    public required GranteeType GranteeType { get; set; }
    public Guid? GranteeNetworkId { get; set; }
    public Guid? GranteeRoleId { get; set; }
    public Guid? GranteePermissionId { get; set; }
    public Guid? GranteeUserProxyId { get; set; }
    public ContentCapabilities Capabilities { get; set; } = ContentCapabilities.View;

    // Self-expiring share (§2.4) — null means no automatic expiry, only a manual revoke.
    public DateTime? ExpiresAt { get; set; }
}
