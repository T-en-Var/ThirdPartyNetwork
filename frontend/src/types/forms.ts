import type { SettableEntitlement } from './network';

export type ManageUserForm = {
  roleIds: string[];
  entitlements: SettableEntitlement;
};

export type RoleForm = {
  name: string;
  description: string;
  permissionIds: string[];
  isDefault: boolean;
  entitlements: SettableEntitlement;
};

export type NetworkForm = {
  name: string;
  isPublic: boolean;
  description: string;
  imageUrl?: string;
  redirectURI: string;
};

export type UpdateFile = {
  name: string;
  isPublic: boolean;
  accessLevel?: number;
};

export type ConfirmForm = {
  title: string;
  message: string;
  buttonText: string;
  buttonColor: 'red' | 'blue' | 'green' | 'yellow' | 'gray' | 'white';
  action: () => Promise<void>;
};

export type CreateUserContentForm = {
  networkId: string;
  accessLevel: number;
};
