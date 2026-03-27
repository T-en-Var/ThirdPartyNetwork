import type { Network, NetworkPermissionCollection } from '@/types';

export class ClaimChecker {
  permissions = {
    Administrator: 'Administrator',
    ReadNetwork: 'Read Network',
    ManageNetwork: 'Manage Network',
    ReadAccess: 'Read Access',
    ManageAccess: 'Manage Access',
    ReadPermission: 'Read Permission',
    ManagePermission: 'Manage Permission',
    ReadRole: 'Read Role',
    ManageRole: 'Manage Role',
    IsUserOwner: 'IsUserOwner',
    IsNetworkUserOwner: 'IsNetworkUserOwner',
    IsUserProxyOwner: 'IsUserProxyOwner',
    ReadUser: 'Read User',
    ManageUser: 'Manage User',
    ReadCustomPage: 'Read CustomPage',
    ManageCustomPage: 'Manage CustomPage',
    ReadPageBlock: 'Read PageBlock',
    ManagePageBlock: 'Manage PageBlock',
    ReadFile: 'Read File',
    ManageFile: 'Manage File',
    ReadConfiguration: 'Read Configuration',
    ManageConfiguration: 'Manage Configuration',
    ReadBlog: 'Read Blog',
    ManageBlog: 'Manage Blog',
  } as const;

  constructor() {}

  private getNetworkCollection = (collections: NetworkPermissionCollection[], network: Network) => {
    return collections.find((collection) => collection.id == network.id);
  };

  private getPermissionValue = (permission: string) => {
    const value = this.permissions[permission as keyof typeof this.permissions];

    if (!value) throw new Error(`Permission '${permission}' does not exist.`);

    return value;
  };

  hasPermission(collections: NetworkPermissionCollection[], network: Network, permission: string) {
    if (!collections) return false;

    const permissionValue = this.getPermissionValue(permission);
    const collection = this.getNetworkCollection(collections, network);

    if (!collection) return false;

    return collection.permissions.some(
      (permission) =>
        permission.name === this.permissions.Administrator || permission.name === permissionValue,
    );
  }
}
