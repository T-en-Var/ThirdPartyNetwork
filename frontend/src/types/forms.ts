export type ManageUserForm = {
  roleIds: string[];
  entitlements: object;
};

export type RoleForm = {
  name: string;
  description: string;
  permissionIds: string[];
  isDefault: boolean;
  entitlements: object;
}

export type NetworkForm = {
  name: string;
  isPublic: boolean;
  description: string,
  imageUrl?: string,
  redirectURI: string,
}

export type EditFileForm = {
  name: string; 
  isPublic: boolean;
  accessLevel?: number;
}
