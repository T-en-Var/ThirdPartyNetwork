import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';
import type { PageLightDto } from '../CustomPage/PageDto';

export interface PageBlockDto extends UserContentBaseDto {
  parentPageId: string | null;
  text: string;
  position: number;
  data: unknown;
  customPageId: string;
  page: PageLightDto;
}

export interface PageBlockLightDto extends UserContentBaseLightDto {
  parentPageId: string | null;
  text: string;
  position: number;
  customPageId: string;
  data: unknown;
}
