import type { UserProxyLightDto, UserProxyLightWithNetworksDto } from '../UserProxy/UserProxyDto';

export interface UserDto {
  id: string;
  createdOn: string;
  userProxies: UserProxyLightDto[];
}

export interface UserWithNetworksDto {
  id: string;
  createdOn: string;
  userProxies: UserProxyLightWithNetworksDto[];
}

export interface UserLightDto {
  id: string;
  createdOn: string;
}
