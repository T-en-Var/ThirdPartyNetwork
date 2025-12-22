import type { NetworkUser } from "./networkUser";

export interface NetworkFile {
  id: string;
  url: string;
  format: string;
  sizeBytes: number;
  width?: number;
  height?: number;
  duration?: string; // "HH:mm:ss"
  mediaType: string;
  name: string;
  createdAt: Date;
  isPublic: boolean;
  author: NetworkUser;
  networkId: string;
  isSystemProtected: boolean;
  accessLevel: number;
}

export type NetworkFileCreate = Pick<NetworkFile, 'url' | 'format' | 'sizeBytes' | 'mediaType' | 'name' | 'isPublic'>;

export interface NetworkFileLink {
  id: string;
}
