import type { ContentKind } from '../Enums/ContentKind';
import type { ModerationFlagStatus } from '../Enums/ModerationFlagStatus';

export interface ContentModerationFlagDto {
  id: string;
  contentKind: ContentKind;
  contentId: string;
  networkId: string;
  flaggedByNetworkUserId: string | null;
  reason: string;
  status: ModerationFlagStatus;
  hiddenInNetwork: boolean;
  createdOn: string;
  updatedAt: string;
}
