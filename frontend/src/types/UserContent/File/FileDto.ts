import type { UserContentBaseDto, UserContentBaseLightDto } from '../UserContentBase/UserContentBaseDto';

export interface FileDto extends UserContentBaseDto {
  bucket: string;
  objectKey: string;
  format: string | null;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  duration: string | null;
  mediaType: string;
  name: string;
  isSystemProtected: boolean;
}

export interface FileLightDto extends UserContentBaseLightDto {
  bucket: string;
  objectKey: string;
  format: string | null;
  sizeBytes: number;
  width: number | null;
  height: number | null;
  duration: string | null;
  mediaType: string;
  name: string;
  isSystemProtected: boolean;
}
