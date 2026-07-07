import type { UserProxy } from './userProxy';

export interface User {
  id: string;
  createdOn: Date;
  userProxies: UserProxy[];
}

export type CreateUser = Record<string, never>;

export type UpdateUser = Record<string, never>;

export interface UserMetrics {
  userId: string;
  fileCount: number;
  fileStorage: number;
  blogCount: number;
  configurationCount: number;
  customPageCount: number;
  customPageBlockCount: number;
  customPageBlockSize: number;
}
