import type { CreateUserContentBaseDto } from '../UserContentBase/CreateUserContentBaseDto';

export interface CreateBlogDto extends CreateUserContentBaseDto {
  title: string;
  summary: string;
  coverImageId: string | null;
  body: unknown;
}
