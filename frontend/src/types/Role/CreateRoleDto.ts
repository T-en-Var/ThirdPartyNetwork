export interface CreateRoleDto {
  name: string;
  description: string;
  isDefault: boolean;
  entitlements: unknown;
}
