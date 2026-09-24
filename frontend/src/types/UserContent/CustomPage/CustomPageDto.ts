import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';
import type { PageBlockLightDto } from '../PageBlock/PageBlockDto';

export interface CustomPageDto extends UserContentBaseDto {
  name: string;
  slug: string;
  blocks: PageBlockLightDto[];
}

export interface CustomPageLightDto extends UserContentBaseLightDto {
  name: string;
  slug: string;
}
