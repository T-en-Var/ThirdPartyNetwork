import type { NetworkFile } from "./files";
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
  accessIncomplete: boolean,
  networkUserAccesses: NetworkUserAccess[],
  networkUserRoles: NetworkUserRole[],
  createdOn: Date,
  files: NetworkFile[]
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface CreateNetworkUser {
  
}
