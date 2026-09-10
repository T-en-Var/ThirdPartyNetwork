import type { FileDto, FileLightDto } from '../UserContent/File/FileDto';
import type { UserLightDto } from '../User/UserDto';
import type { NetworkUserLightWithNetworkDto } from '../NetworkUser/NetworkUserDto';

export interface UserProxyDto {
  id: string;
  username: string | null;
  firstName: string | null;
  lastName: string | null;
  email: string | null;
  imageFile: FileDto | null;
  isDefault: boolean;
  hasPassword: boolean;
  createdOn: string;
  user: UserLightDto;
  networkUsers: NetworkUserLightWithNetworkDto[];
}

export interface UserProxyLightDto {
  id: string;
  username: string | null;
  firstName: string | null;
  lastName: string | null;
  email: string | null;
  imageFile: FileDto | null;
  isDefault: boolean;
  hasPassword: boolean;
  createdOn: string;
}

export interface UserProxyLightShortProfileDto {
  id: string;
  username: string | null;
  imageFile: FileLightDto | null;
  isDefault: boolean;
  createdOn: string;
}

export interface UserProxyLightNoCredentialsDto {
  id: string;
  isDefault: boolean;
  createdOn: string;
}

export interface UserProxyLightWithNetworksDto extends UserProxyLightDto {
  networkUsers: NetworkUserLightWithNetworkDto[];
}
