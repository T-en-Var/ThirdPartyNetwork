import type { NetworkUser } from "./networkUser";

export interface Configuration {
  id: string;
  key: string;
  value: object;
  networkId: string;
  author: NetworkUser;
  authorId: string;
  accessLevel: number;
}

export interface CreateConfiguration {
  key: string;
  value: object;
  accessLevel: number;
}