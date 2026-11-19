export interface FeatureFlagDto {
  allowFiles: boolean | null;
  fileCountLimit: number | null;
  fileSizeLimit: number | null;
  /** in KB */
  fileStorageLimit: number | null;

  allowBlogs: boolean | null;
  blogCountLimit: number | null;

  allowConfigurations: boolean | null;
  configurationCountLimit: number | null;

  allowCustomPages: boolean | null;
  customPageCountLimit: number | null;
  customPageBlockCountLimit: number | null;
  /** Character count */
  customPageBlockSizeLimit: number | null;
}
