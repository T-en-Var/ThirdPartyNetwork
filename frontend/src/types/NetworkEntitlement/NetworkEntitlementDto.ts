export interface NetworkEntitlementDto {
  networkId: string;
  allowFiles: boolean;
  fileCountLimit: number;
  /** in KB */
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
