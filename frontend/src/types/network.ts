import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";

export interface Network {
  id: string,
  name: string,
  isPublic: boolean,
  networkAccesses: NetworkAccess[],
  roles: Role[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
}

export interface CreateNetwork {
  name: string,
  isPublic: boolean,
}

export interface UpdateNetwork {
  name: string,
  isPublic: boolean,
}
