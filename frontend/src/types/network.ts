import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";
import type { NetworkFile, NetworkFileLink } from "./files";

export interface Network {
  id: string,
  name: string,
  description: string,
  imageFile?: NetworkFile,
  isPublic: boolean,
  redirectURI: string,
  createdOn: Date,
  networkAccesses: NetworkAccess[],
  roles: Role[],
  networkUsers: NetworkUser[],
  isSystemProtected: boolean,
  // files: NetworkFile[],
  // customPages: CustomPage[]
}

export interface CreateNetwork {
  name: string,
  isPublic: boolean,
  description: string,
  fileLink?: NetworkFileLink,
  redirectURI: string,
}

export interface NetworkUpdate {
  name: string,
  isPublic: boolean,
  description: string,
  fileLink?: NetworkFileLink,
  redirectURI: string,
}
