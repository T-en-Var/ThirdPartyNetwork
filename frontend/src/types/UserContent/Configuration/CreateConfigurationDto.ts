import type { CreateUserContentBaseDto } from '../UserContentBase/CreateUserContentBaseDto';

export interface CreateConfigurationDto extends CreateUserContentBaseDto {
  key: string;
  value: unknown;
}
