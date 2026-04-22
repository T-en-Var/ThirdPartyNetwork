import type { UserProxy } from './userProxy';

export interface User {
  id: string;
  createdOn: Date;
  userProxies: UserProxy[];
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface CreateUser {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UpdateUser {}

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
