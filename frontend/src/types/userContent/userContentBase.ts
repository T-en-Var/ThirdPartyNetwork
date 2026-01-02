import type { NetworkUser } from "../networkUser";

export interface UserContentBase {
  networkId: string;
  authorId: string;
  author: NetworkUser;
  accessLevel: number;
}

export interface UserContentCreateBase {
  accessLevel: number;
}