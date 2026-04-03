import type { NetworkAccess } from './networkAccess';
import type { Role } from './role';
import type { NetworkUser } from './networkUser';
import type { NetworkFile, NetworkFileLink } from './userContent/files';

export interface EntitlementLimits {
  fileCountLimit: number;
  fileSizeLimit: number; // in KB
  fileStorageLimit: number; // in KB

  blogCountLimit: number;

  configurationCountLimit: number;

  customPageCountLimit: number;
  customPageBlockCountLimit: number;
  customPageBlockSizeLimit: number; // in chars
}

export interface NetworkEntitlement extends EntitlementLimits {
  networkId: string;

  allowFiles: boolean;
  allowBlogs: boolean;
  allowConfigurations: boolean;
  allowCustomPages: boolean;
}

export interface SettableEntitlement {
  allowFiles?: boolean;
  fileCountLimit?: number;
  fileSizeLimit?: number;
  fileStorageLimit?: number;

  allowBlogs?: boolean;
  blogCountLimit?: number;

  allowConfigurations?: boolean;
  configurationCountLimit?: number;

  allowCustomPages?: boolean;
  customPageCountLimit?: number;
  customPageBlockCountLimit?: number;
  customPageBlockSizeLimit?: number;
}

export interface SettableEntitlementForm extends SettableEntitlement {
  setAllowFiles?: boolean;
  setFileCountLimit?: boolean;
  setFileSizeLimit?: boolean;
  setFileStorageLimit?: boolean;

  setAllowBlogs?: boolean;
  setBlogCountLimit?: boolean;

  setAllowConfigurations?: boolean;
  setConfigurationCountLimit?: boolean;

  setAllowCustomPages?: boolean;
  setCustomPageCountLimit?: boolean;
  setCustomPageBlockCountLimit?: number;
  setCustomPageBlockSizeLimit?: number;
}

export interface Network {
  id: string;
  name: string;
  description: string;
  imageFile?: NetworkFile;
  isPublic: boolean;
  redirectURI: string;
  createdOn: Date;
  networkAccesses: NetworkAccess[];
  roles: Role[];
  networkUsers: NetworkUser[];
  isSystemProtected: boolean;
  entitlement?: NetworkEntitlement;
}

export interface CreateNetwork {
  name: string;
  isPublic: boolean;
  description: string;
  fileLink?: NetworkFileLink;
  redirectURI: string;
}

export interface NetworkUpdate {
  name: string;
  isPublic: boolean;
  description: string;
  fileLink?: NetworkFileLink;
  redirectURI: string;
}

export interface NetworkMetrics {
  networkId: string;
  fileCount: number;
  fileStorage: number;
  blogCount: number;
  configurationCount: number;
  customPageCount: number;
  customPageBlockCount: number;
  customPageBlockSize: number;
}
