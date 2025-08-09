import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";
import type { NetworkFile } from "./files";
// import type { File } from "./files";

export interface Network {
  id: string,
  name: string,
  description: string,
  imageUrl?: string,
  isPublic: boolean,
  redirectURI: string,
  createdOn: Date,
  networkAccesses: NetworkAccess[],
  roles: Role[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
  files: NetworkFile[],
}

export interface CreateNetwork {
  name: string,
  isPublic: boolean,
  description: string,
  imageUrl?: string,
  redirectURI: string,
}

export interface UpdateNetwork {
  name: string,
  isPublic: boolean,
  description: string,
  imageUrl?: string,
  redirectURI: string,
}
