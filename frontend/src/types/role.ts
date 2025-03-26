import type { NetworkUserRole } from "./networkUserRole";
import type { RolePermission } from "./rolePermission";

export interface Role {
  id: string,
  name: string,
  description: string,
  rolePermissions: RolePermission[],
  networkUserRoles: NetworkUserRole[],
}