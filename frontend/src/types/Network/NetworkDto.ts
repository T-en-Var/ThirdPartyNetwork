import type { FileDto } from '../UserContent/File/FileDto';
import type { ContentKindFlags } from '../Enums/ContentKindFlags';
import type { NetworkAccessNtADto } from '../NetworkAccess/NetworkAccessDto';
import type { RoleLightDto } from '../Role/RoleDto';
import type { NetworkUserLightUserInfoDto } from '../NetworkUser/NetworkUserDto';
import type { NetworkEntitlementDto } from '../NetworkEntitlement/NetworkEntitlementDto';

export interface NetworkDto {
  id: string;
  name: string;
  description: string;
  redirectURI: string;
  imageFile: FileDto | null;
  createdOn: string;
  isPublic: boolean;
  externalContentKinds: ContentKindFlags;
  networkAccesses: NetworkAccessNtADto[];
  roles: RoleLightDto[];
  networkUsers: NetworkUserLightUserInfoDto[];
  entitlement: NetworkEntitlementDto | null;
  isSystemProtected: boolean;
}

export interface NetworkLightDto {
  id: string;
  name: string;
  description: string;
  redirectURI: string;
  imageFile: FileDto | null;
  createdOn: string;
  isPublic: boolean;
  entitlement: NetworkEntitlementDto | null;
  isSystemProtected: boolean;
}
