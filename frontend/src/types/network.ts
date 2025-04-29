import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";

export interface Network {
  id: string,
  name: string,
  description: string,
  imageUrl?: string,
  isPublic: boolean,
  createdOn: Date,
  networkAccesses: NetworkAccess[],
  roles: Role[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
}

export interface CreateNetwork {
  name: string,
  isPublic: boolean,
  description: string,
  imageUrl?: string,
}

export interface UpdateNetwork {
  name: string,
  isPublic: boolean,
  description: string,
  imageUrl?: string,
}
