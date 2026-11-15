import type { GranteeType } from '../Enums/GranteeType';
import type { ContentCapabilities } from '../Enums/ContentCapabilities';

export interface CreateContentAccessGrantDto {
  granteeType: GranteeType;
  granteeNetworkId: string | null;
  granteeRoleId: string | null;
  granteePermissionId: string | null;
  granteeUserProxyId: string | null;
  capabilities: ContentCapabilities;
  expiresAt: string | null;
}
