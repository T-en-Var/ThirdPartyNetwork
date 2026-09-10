import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';

export interface ConfigurationDto extends UserContentBaseDto {
  id: string;
  key: string;
  value: unknown;
}

export interface ConfigurationLightDto extends UserContentBaseLightDto {
  key: string;
  value: unknown;
}
