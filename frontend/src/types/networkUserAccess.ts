import type { Access } from "./access";
import type { NetworkUser } from "./networkUser";

export interface NetworkUserAccess {
  accessId: string,
  networkUserId: string,
  isAccepted: boolean,
  networkUser: NetworkUser,
  access: Access
}