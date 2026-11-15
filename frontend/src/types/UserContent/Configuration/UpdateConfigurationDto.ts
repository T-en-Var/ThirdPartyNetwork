import type { UpdateUserContentBaseDto } from '../UserContentBase/UpdateUserContentBaseDto';

export interface UpdateConfigurationDto extends UpdateUserContentBaseDto {
  key: string | null;
  value: unknown;
}
