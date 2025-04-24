export type ManageUserForm = {
  roleIds: string[];
};

export type RoleForm = {
  name: string;
  description: string;
  permissionIds: string[];
  isDefault: boolean;
}

export type NetworkForm = { 
  name: string;
  isPublic: boolean;
}
