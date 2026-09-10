import type { NetworkLightDto } from '../Network/NetworkDto';
import type { RolePermissionPtRDto } from '../RolePermission/RolePermissionDto';

export interface PermissionDto {
  id: string;
  name: string;
  description: string;
  network: NetworkLightDto | null;
  rolePermissions: RolePermissionPtRDto[];
}

export interface PermissionLightDto {
  id: string;
  name: string;
  description: string;
}

export interface PermissionShortDto {
  id: string;
  name: string;
}
