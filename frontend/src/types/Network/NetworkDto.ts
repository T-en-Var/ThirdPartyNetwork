import type { FileDto } from '../UserContent/File/FileDto';
import type { ContentKindFlags } from '../Enums/ContentKindFlags';
import type { NetworkAccessNtADto } from '../NetworkAccess/NetworkAccessDto';
import type { RoleLightDto } from '../Role/RoleDto';
import type { NetworkUserLightUserInfoDto } from '../NetworkUser/NetworkUserDto';
import type { NetworkFeatureFlagDto } from '../NetworkFeatureFlag/NetworkFeatureFlagDto';

export interface NetworkDto {
  id: string;
  name: string;
  description: string;
  redirectURI: string;
  imageFile: FileDto | null;
  createdOn: Date;
  isPublic: boolean;
  externalContentKinds: ContentKindFlags;
  networkAccesses: NetworkAccessNtADto[];
  roles: RoleLightDto[];
  networkUsers: NetworkUserLightUserInfoDto[];
  featureFlag: NetworkFeatureFlagDto | null;
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
  featureFlag: NetworkFeatureFlagDto | null;
  isSystemProtected: boolean;
}
