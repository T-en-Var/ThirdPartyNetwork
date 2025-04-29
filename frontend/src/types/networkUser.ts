import type { Network } from "./network";
import type { NetworkUserAccess } from "./networkUserAccess";
import type { NetworkUserRole } from "./networkUserRole";
import type { UserProxy } from "./userProxy";

export interface NetworkUser {
  id: string,
  networkId: string,
  userProxyId: string,
  network: Network,
  userProxy: UserProxy,
  networkUserAccesses: NetworkUserAccess[],
  networkUserRoles: NetworkUserRole[],
  createdOn: Date
}

export interface CreateNetworkUser {
  
}
