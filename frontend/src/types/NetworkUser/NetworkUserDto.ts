import type { UserProxyLightDto, UserProxyLightShortProfileDto } from '../UserProxy/UserProxyDto';
import type { NetworkLightDto } from '../Network/NetworkDto';
import type { NetworkUserAccessNtADto } from '../NetworkUserAccess/NetworkUserAccessDto';
import type { NetworkUserRoleNtRDto } from '../NetworkUserRole/NetworkUserRoleDto';

export interface NetworkUserDto {
  id: string;
  networkId: string;
  userProxyId: string;
  userProxy: UserProxyLightDto;
  network: NetworkLightDto;
  accessIncomplete: boolean;
  createdOn: string;
  networkUserAccesses: NetworkUserAccessNtADto[];
  networkUserRoles: NetworkUserRoleNtRDto[];
  featureFlags: unknown;
}

export interface NetworkUserLightDto {
  id: string;
  networkId: string;
  userProxyId: string;
  userProxy: UserProxyLightShortProfileDto;
  accessIncomplete: boolean;
  createdOn: string;
  featureFlags: unknown;
}

export interface NetworkUserLightUserInfoDto {
  id: string;
  networkId: string;
  userProxyId: string;
  accessIncomplete: boolean;
  createdOn: string;
  featureFlags: unknown;
  userProxy: UserProxyLightDto;
  networkUserAccesses: NetworkUserAccessNtADto[];
  networkUserRoles: NetworkUserRoleNtRDto[];
}

export interface NetworkUserLightWithNetworkDto {
  id: string;
  networkId: string;
  userProxyId: string;
  accessIncomplete: boolean;
  createdOn: string;
  featureFlags: unknown;
  network: NetworkLightDto;
}

export interface NetworkUserUserProxyCascadeDto extends NetworkUserLightWithNetworkDto {
  networkUserAccesses: NetworkUserAccessNtADto[];
  networkUserRoles: NetworkUserRoleNtRDto[];
} 
