import type { Network } from "./network";
import type { NetworkUserRole } from "./networkUserRole";
import type { RolePermission } from "./rolePermission";

export interface Role {
  id: string,
  name: string,
  description: string,
  isDefault: boolean,
  rolePermissions: RolePermission[],
  networkUserRoles: NetworkUserRole[],
  network: Network,
}

export interface CreateRole {
  name: string,
  description: string,
  isDefault: boolean,
}

export interface UpdateRole {
  name: string,
  description: string,
  isDefault: boolean,
}
