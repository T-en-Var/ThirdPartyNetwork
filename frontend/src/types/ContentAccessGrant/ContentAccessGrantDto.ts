import type { ContentKind } from '../Enums/ContentKind';
import type { GranteeType } from '../Enums/GranteeType';
import type { ContentCapabilities } from '../Enums/ContentCapabilities';

export interface ContentAccessGrantDto {
  id: string;
  contentKind: ContentKind;
  contentId: string;
  granteeType: GranteeType;
  granteeNetworkId: string | null;
  granteeRoleId: string | null;
  granteePermissionId: string | null;
  granteeUserProxyId: string | null;
  capabilities: ContentCapabilities;
  grantedByNetworkUserId: string | null;
  expiresAt: string | null;
  revokedAt: string | null;
  createdOn: string;
}
