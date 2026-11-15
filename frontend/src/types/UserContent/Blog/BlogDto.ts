import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';
import type { FileLightDto } from '../File/FileDto';

export interface BlogDto extends UserContentBaseDto {
  title: string;
  slug: string;
  summary: string;
  coverImage: FileLightDto | null;
  body: unknown;
  publishedAt: string | null;
}

export interface BlogLightDto extends UserContentBaseLightDto {
  title: string;
  slug: string;
  summary: string;
  coverImage: FileLightDto | null;
  body: unknown;
  publishedAt: string | null;
}
