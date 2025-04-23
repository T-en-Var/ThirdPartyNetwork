import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";

export interface Network {
  id: string,
  name: string,
  networkAccesses: NetworkAccess[],
  roles: Role[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
}