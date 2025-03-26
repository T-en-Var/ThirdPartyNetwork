import type { Network } from "./network";
import type { NetworkUserAccess } from "./networkUserAccess";
import type { NetworkUserRole } from "./networkUserRole";
import type { UserProxy } from "./userProxy";

export interface NetworkUser {
  id: string,
  network: Network,
  userProxy: UserProxy,
  networkUserAccesses: NetworkUserAccess[],
  networkUserRoles: NetworkUserRole[],
}