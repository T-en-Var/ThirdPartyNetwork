import type { NetworkPermissionCollection } from '@/types';

export class ClaimChecker {
  permissions = {
    'Administrator': 'Administrator',
    'Read Network': 'Read Network',
    'Manage Network': 'Manage Network',
    'Read Access': 'Read Access',
    'Manage Access': 'Manage Access',
    'Read Permission': 'Read Permission',
    'Manage Permission': 'Manage Permission',
    'Read Role': 'Read Role',
    'Manage Role': 'Manage Role',
    'IsUserOwner': 'IsUserOwner',
    'IsNetworkUserOwner': 'IsNetworkUserOwner',
    'IsUserProxyOwner': 'IsUserProxyOwner',
    'Read User': 'Read User',
    'Manage User': 'Manage User',
    'Read CustomPage': 'Read CustomPage',
    'Manage CustomPage': 'Manage CustomPage',
    'Read PageBlock': 'Read PageBlock',
    'Manage PageBlock': 'Manage PageBlock',
    'Read File': 'Read File',
    'Manage File': 'Manage File',
    'Read Configuration': 'Read Configuration',
    'Manage Configuration': 'Manage Configuration',
    'Read Blog': 'Read Blog',
    'Manage Blog': 'Manage Blog',
  } as const;

  constructor() {}

  private getNetworkCollection = (collections: NetworkPermissionCollection[], networkId: string) => {
    return collections.find((collection) => collection.id == networkId);
  };

  private getPermissionValue = (permission: string) => {
    const value = this.permissions[permission as keyof typeof this.permissions];

    if (!value) throw new Error(`Permission '${permission}' does not exist.`);

    return value;
  };

  hasPermission(collections: NetworkPermissionCollection[], networkId: string, permission: string) {
    if (!collections) return false;

    const permissionValue = this.getPermissionValue(permission);
    const collection = this.getNetworkCollection(collections, networkId);

    if (!collection) return false;

    return collection.permissions.some(
      (permission) =>
        permission.name === this.permissions.Administrator || permission.name === permissionValue,
    );
  }
}
