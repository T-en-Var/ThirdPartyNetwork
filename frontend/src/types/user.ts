import type { UserProxy } from "./userProxy";

export interface User {
  id: string,
  createdOn: Date,
  userProxies: UserProxy[],
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface CreateUser {
  
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UpdateUser {

}
