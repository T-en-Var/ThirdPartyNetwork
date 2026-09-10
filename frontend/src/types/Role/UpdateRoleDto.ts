export interface UpdateRoleDto {
  name: string;
  description: string;
  isDefault: boolean;
  entitlements: unknown;
}
