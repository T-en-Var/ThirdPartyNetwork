import type { UserContentBase, UserContentCreateBase } from "./userContentBase";

export interface NetworkFile extends UserContentBase {
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
  isSystemProtected: boolean;
}

export type NetworkFileCreate = Pick<NetworkFile, 'url' | 'format' | 'sizeBytes' | 'mediaType' | 'name' | 'isPublic'> & UserContentCreateBase;

export interface NetworkFileLink {
  id: string;
}
