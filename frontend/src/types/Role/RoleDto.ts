import type { RolePermissionRtPDto } from '../RolePermission/RolePermissionDto';
import type { NetworkUserRoleRtNDto } from '../NetworkUserRole/NetworkUserRoleDto';
import type { NetworkLightDto } from '../Network/NetworkDto';
import type { FeatureFlagDto } from '../FeatureFlag/FeatureFlagDto';

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isDefault: boolean;
  rolePermissions: RolePermissionRtPDto[];
  networkUserRoles: NetworkUserRoleRtNDto[];
  featureFlags: FeatureFlagDto;
  network: NetworkLightDto;
}

export interface RoleLightDto {
  id: string;
  name: string;
  description: string;
  isDefault: boolean;
  rolePermissions: RolePermissionRtPDto[];
  featureFlags: FeatureFlagDto;
}
