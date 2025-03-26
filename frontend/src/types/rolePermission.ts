import type { Permission } from "./permission";
import type { Role } from "./role";

export interface RolePermission {
  roleId: string,
  permissionId: string,
  role: Role,
  permission: Permission,
}