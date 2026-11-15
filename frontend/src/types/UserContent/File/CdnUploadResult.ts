export interface CdnUploadResult {
  bucket: string;
  objectKey: string;
  bytes: number;
  resourceType: string;
  format: string;
  width: number | null;
  height: number | null;
  duration: string | null;
}
