export interface EntitlementLimits {
  fileCountLimit: number;
  fileSizeLimit: number; // in KB
  fileStorageLimit: number; // in KB

  blogCountLimit: number;

  configurationCountLimit: number;

  customPageCountLimit: number;
  customPageBlockCountLimit: number;
  customPageBlockSizeLimit: number; // in chars
}

export interface NetworkEntitlementDto extends EntitlementLimits {
  networkId: string;

  allowFiles: boolean;
  allowBlogs: boolean;
  allowConfigurations: boolean;
  allowCustomPages: boolean;
}

export interface SettableEntitlement {
  allowFiles?: boolean;
  fileCountLimit?: number;
  fileSizeLimit?: number;
  fileStorageLimit?: number;

  allowBlogs?: boolean;
  blogCountLimit?: number;

  allowConfigurations?: boolean;
  configurationCountLimit?: number;

  allowCustomPages?: boolean;
  customPageCountLimit?: number;
  customPageBlockCountLimit?: number;
  customPageBlockSizeLimit?: number;
}

export interface SettableEntitlementForm extends SettableEntitlement {
  setAllowFiles?: boolean;
  setFileCountLimit?: boolean;
  setFileSizeLimit?: boolean;
  setFileStorageLimit?: boolean;

  setAllowBlogs?: boolean;
  setBlogCountLimit?: boolean;

  setAllowConfigurations?: boolean;
  setConfigurationCountLimit?: boolean;

  setAllowCustomPages?: boolean;
  setCustomPageCountLimit?: boolean;
  setCustomPageBlockCountLimit?: boolean;
  setCustomPageBlockSizeLimit?: boolean;
}
