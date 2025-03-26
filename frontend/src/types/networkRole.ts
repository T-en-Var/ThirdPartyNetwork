import type { Network } from "./network";
import type { Role } from "./role";

export interface NetworkRole {
  networkId: string,
  roleId: string,
  role: Role,
  network: Network,
}