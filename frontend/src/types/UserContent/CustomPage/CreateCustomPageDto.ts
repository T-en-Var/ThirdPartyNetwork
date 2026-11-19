import type { CreateUserContentBaseDto } from '../UserContentBase/CreateUserContentBaseDto';

export interface CreateCustomPageDto extends CreateUserContentBaseDto {
  name: string;
  slug: string;
}
