import type { SettableFeatureFlag } from './NetworkFeatureFlag/NetworkFeatureFlagDto';

export type ManageUserForm = {
  roleIds: string[];
  featureFlags: SettableFeatureFlag;
};

export type RoleForm = {
  name: string;
  description: string;
  permissionIds: string[];
  isDefault: boolean;
  featureFlags: SettableFeatureFlag;
};

export type NetworkForm = {
  name: string;
  isPublic: boolean;
  description: string;
  imageUrl?: string;
  redirectURI: string;
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
};
