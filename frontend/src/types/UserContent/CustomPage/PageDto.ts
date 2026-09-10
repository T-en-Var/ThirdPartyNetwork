import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';
import type { PageBlockLightDto } from '../PageBlock/PageBlockDto';

export interface PageDto extends UserContentBaseDto {
  name: string;
  slug: string;
  pages: PageBlockLightDto[];
}

export interface PageLightDto extends UserContentBaseLightDto {
  name: string;
  slug: string;
}
