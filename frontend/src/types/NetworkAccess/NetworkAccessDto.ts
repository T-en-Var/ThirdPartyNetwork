import type { AccessLightDto } from '../Access/AccessDto';
import type { NetworkLightDto } from '../Network/NetworkDto';

export interface NetworkAccessDto {
  networkId: string;
  accessId: string;
  isRequired: boolean;
  access: AccessLightDto;
  network: NetworkLightDto;
}

export interface NetworkAccessNtADto {
  accessId: string;
  isRequired: boolean;
  access: AccessLightDto;
}

export interface NetworkAccessAtNDto {
  networkId: string;
  isRequired: boolean;
  network: NetworkLightDto;
}
