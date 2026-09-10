import type { FileLinkDto } from '../UserContent/File/FileLinkDto';

export interface UpdateUserProxyDto {
  username: string | null;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  password: string | null;
  imageUrl: string | null;
  keepPassword: boolean;
  isDefault: boolean;
  fileLink: FileLinkDto | null;
}
