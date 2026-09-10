import type { RolePermissionRtPDto } from '../RolePermission/RolePermissionDto';
import type { NetworkUserRoleRtNDto } from '../NetworkUserRole/NetworkUserRoleDto';
import type { NetworkLightDto } from '../Network/NetworkDto';

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isDefault: boolean;
  rolePermissions: RolePermissionRtPDto[];
  networkUserRoles: NetworkUserRoleRtNDto[];
  entitlements: unknown;
  network: NetworkLightDto;
}

export interface RoleLightDto {
  id: string;
  name: string;
  description: string;
  isDefault: boolean;
  rolePermissions: RolePermissionRtPDto[];
  entitlements: unknown;
}
