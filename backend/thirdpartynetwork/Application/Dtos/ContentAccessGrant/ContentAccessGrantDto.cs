using thirdpartynetwork.Core.Enum;

namespace thirdpartynetwork.Application.Dtos.ContentAccessGrant;

public class ContentAccessGrantDto
{
    public Guid Id { get; set; }
    public ContentKind ContentKind { get; set; }
    public Guid ContentId { get; set; }
    public GranteeType GranteeType { get; set; }
    public Guid? GranteeNetworkId { get; set; }
    public Guid? GranteeRoleId { get; set; }
    public Guid? GranteePermissionId { get; set; }
    public Guid? GranteeUserProxyId { get; set; }
    public ContentCapabilities Capabilities { get; set; }
    public Guid? GrantedByNetworkUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedOn { get; set; }
}
