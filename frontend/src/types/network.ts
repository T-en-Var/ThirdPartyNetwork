import type { NetworkAccess } from "./networkAccess";
import type { Role } from "./role";
import type { NetworkUser } from "./networkUser";
import type { NetworkFile, NetworkFileLink } from "./userContent/files";


export interface NetworkEntitlement {
  networkId: string;

  allowFiles: boolean;
  fileCountLimit: number;
  fileSizeLimit: number; // in KB
  fileStorageLimit: number; // in KB

  allowBlogs: boolean;
  blogCountLimit: number;

  allowConfigurations: boolean;
  configurationCountLimit: number;

  allowCustomPages: boolean;
  customPageCountLimit: number;
}

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
  entitlement?: NetworkEntitlement,
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
