import type { UserProxy } from "./userProxy";

export interface User {
  id: string,
  createdOn: Date,
  userProxies: UserProxy[],
}