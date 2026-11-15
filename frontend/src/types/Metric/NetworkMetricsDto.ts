export interface NetworkMetricsDto {
  networkId: string;
  fileCount: number;
  /** in KB */
  fileStorage: number;
  blogCount: number;
  configurationCount: number;
  customPageCount: number;
  customPageBlockCount: number;
  /** in Chars */
  customPageBlockSize: number;
}
