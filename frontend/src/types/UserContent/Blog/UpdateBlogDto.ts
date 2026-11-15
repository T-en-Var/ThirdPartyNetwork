import type { UpdateUserContentBaseDto } from '../UserContentBase/UpdateUserContentBaseDto';

export interface UpdateBlogDto extends UpdateUserContentBaseDto {
  title: string;
  summary: string;
  coverImageId: string | null;
  body: unknown;
}
