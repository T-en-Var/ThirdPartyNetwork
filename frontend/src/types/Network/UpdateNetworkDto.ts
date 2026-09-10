import type { FileLinkDto } from '../UserContent/File/FileLinkDto';
import type { ContentKindFlags } from '../Enums/ContentKindFlags';

export interface UpdateNetworkDto {
  name: string;
  isPublic: boolean;
  description: string;
  redirectURI: string;
  fileLink: FileLinkDto | null;
  externalContentKinds: ContentKindFlags | null;
}
