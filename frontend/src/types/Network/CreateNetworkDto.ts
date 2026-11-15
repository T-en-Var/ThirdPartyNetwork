import type { FileLinkDto } from '../UserContent/File/FileLinkDto';

export interface CreateNetworkDto {
  name: string;
  isPublic: boolean;
  description: string;
  redirectURI: string;
  fileLink: FileLinkDto | null;
}
