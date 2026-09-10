import type { PermissionShortDto } from '../Permission/PermissionDto';

// Note: filename matches the source file "MyPermissionsDto.cs";
// the class itself is named NetworkPermissionsCollectionDto.
export interface NetworkPermissionsCollectionDto {
  id: string;
  name: string;
  permissions: PermissionShortDto[];
}
