export interface CreateNetworkEntitlementDto {
  allowFiles: boolean;
  fileCountLimit: number;
  fileSizeLimit: number;
  /** in KB */
  fileStorageLimit: number;
  allowBlogs: boolean;
  blogCountLimit: number;
  allowConfigurations: boolean;
  configurationCountLimit: number;
  allowCustomPages: boolean;
  customPageCountLimit: number;
  customPageBlockCountLimit: number;
  customPageBlockSizeLimit: number;
}
