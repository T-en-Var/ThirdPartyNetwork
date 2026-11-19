import type { NetworkAccessAtNDto } from '../NetworkAccess/NetworkAccessDto';
import type { NetworkUserAccessAtNDto } from '../NetworkUserAccess/NetworkUserAccessDto';

export interface AccessDto {
  id: string;
  name: string;
  description: string;
  /** [JsonIgnore] in source — not present in serialized JSON */
  networkAccesses: NetworkAccessAtNDto[];
  /** [JsonIgnore] in source — not present in serialized JSON */
  networkUserAccesses: NetworkUserAccessAtNDto[];
}

export interface AccessLightDto {
  id: string;
  name: string;
  description: string;
}
