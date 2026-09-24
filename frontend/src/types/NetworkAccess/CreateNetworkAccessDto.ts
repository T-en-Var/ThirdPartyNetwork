import type { AccessLightDto } from "../Access/AccessDto";

export interface CreateNetworkAccessDto {
  isRequired: boolean;
}

export interface CreateNetworkAccessForm {
  networkId: string;
  isRequired: boolean;
  access?: AccessLightDto;
}
