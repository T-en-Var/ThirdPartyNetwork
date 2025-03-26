import type { NetworkAccess } from "./networkAccess";
import type { NetworkUserAccess } from "./networkUserAccess";

export interface Access {
  id: string,
  name: string,
  description: string,
  networkAccesses: NetworkAccess[],
  networkUserAccesses: NetworkUserAccess[],
}