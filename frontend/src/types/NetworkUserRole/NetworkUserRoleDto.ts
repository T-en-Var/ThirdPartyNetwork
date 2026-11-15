import type { NetworkUserLightDto } from '../NetworkUser/NetworkUserDto';
import type { RoleLightDto } from '../Role/RoleDto';

export interface NetworkUserRoleDto {
  networkUser: NetworkUserLightDto;
  role: RoleLightDto;
}

export interface NetworkUserRoleNtRDto {
  role: RoleLightDto;
}

export interface NetworkUserRoleRtNDto {
  networkUser: NetworkUserLightDto;
}
