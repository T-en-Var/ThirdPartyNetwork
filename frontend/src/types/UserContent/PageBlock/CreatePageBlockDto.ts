import type { CreateUserContentBaseDto } from '../UserContentBase/CreateUserContentBaseDto';

export interface CreatePageBlockDto extends CreateUserContentBaseDto {
  parentPageId: string | null;
  text: string;
  position: number;
  data: unknown;
}
