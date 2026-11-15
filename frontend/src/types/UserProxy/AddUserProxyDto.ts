import type { FileLinkDto } from '../UserContent/File/FileLinkDto';

export interface AddUserProxyDto {
  username: string | null;
  firstName: string | null;
  lastName: string | null;
  email: string | null;
  password: string | null;
  fileLink: FileLinkDto | null;
}
