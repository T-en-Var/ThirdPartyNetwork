import type { FileLinkDto } from '../UserContent/File/FileLinkDto';

export interface UpdateUserProxyDto {
  username: string | null;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  password: string | null;
  // imageUrl: string | null;
  keepPassword: boolean;
  isDefault: boolean;
  fileLink: FileLinkDto | null;
}

export interface UpdateUserProxyForm {
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  password_retype?: string;
  keepPassword: boolean;
  isDefault: boolean;
  fileLink?: FileLinkDto;
}
