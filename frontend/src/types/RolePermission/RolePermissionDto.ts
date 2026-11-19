import type { RoleLightDto } from '../Role/RoleDto';
import type { PermissionLightDto } from '../Permission/PermissionDto';

export interface RolePermissionDto {
  roleId: string;
  permissionId: string;
  role: RoleLightDto;
  permission: PermissionLightDto;
}

export interface RolePermissionRtPDto {
  permission: PermissionLightDto;
}

export interface RolePermissionPtRDto {
  role: RoleLightDto;
}
