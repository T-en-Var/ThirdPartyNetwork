import type { RolePermission } from "./rolePermission";

export interface Permission {
  id: string,
  name: string,
  description: string,
  rolePermissions: RolePermission[]
}