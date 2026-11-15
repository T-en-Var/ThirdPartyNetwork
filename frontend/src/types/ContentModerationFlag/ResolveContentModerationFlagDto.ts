import type { ModerationFlagStatus } from '../Enums/ModerationFlagStatus';

export interface ResolveContentModerationFlagDto {
  status: ModerationFlagStatus;
  hiddenInNetwork: boolean | null;
}
