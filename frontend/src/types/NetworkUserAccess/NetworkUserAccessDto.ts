import type { NetworkUserLightDto } from '../NetworkUser/NetworkUserDto';
import type { AccessLightDto } from '../Access/AccessDto';

export interface NetworkUserAccessDto {
  networkUserId: string;
  accessId: string;
  isAccepted: boolean;
  networkUser: NetworkUserLightDto;
  access: AccessLightDto;
}

export interface NetworkUserAccessNtADto {
  isAccepted: boolean;
  access: AccessLightDto;
}

export interface NetworkUserAccessAtNDto {
  isAccepted: boolean;
  networkUser: NetworkUserLightDto;
}
