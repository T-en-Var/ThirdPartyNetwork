import type { Access } from "./access";
import type { NetworkUser } from "./networkUser";

export interface NetworkUserAccess {
  isAccepted: boolean,
  networkUser: NetworkUser,
  access: Access
}