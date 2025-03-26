import type { NetworkAccess } from "./networkAccess";
import type { NetworkRole } from "./networkRole";
import type { NetworkUser } from "./networkUser";

export interface Network {
  id: string,
  name: string,
  networkAccesses: NetworkAccess[],
  networkRoles: NetworkRole[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
}