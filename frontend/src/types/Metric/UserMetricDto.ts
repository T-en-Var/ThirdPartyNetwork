// Note: filename matches the source file "UserMetricDto.cs";
// the class itself is named UserMetricsDto.
export interface UserMetricsDto {
  userId: string;
  fileCount: number;
  /** in KB */
  fileStorage: number;
  blogCount: number;
  configurationCount: number;
  customPageCount: number;
  customPageBlockCount: number;
}
