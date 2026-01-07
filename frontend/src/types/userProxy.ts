import type { NetworkFile, NetworkFileLink } from "./userContent/files";
import type { NetworkUser } from "./networkUser";
import type { User } from "./user";

export interface UserProxy {
  id: string,
  username?: string,
  firstName?: string,
  lastName?: string,
  email?: string,
  imageFile?: NetworkFile,
  createdOn: Date,
  isDefault: boolean,
  user: User
  networkUsers: NetworkUser[],
  hasPassword: boolean,
}

export interface UserLogin {
  email: string;
  password: string;
}

export interface UserSignup {
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface UserProxyCreate {
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  fileLink?: NetworkFileLink;
}

export interface UserProxyUpdate {
  id: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  keepPassword: boolean;
  isDefault: boolean;
  fileLink?: NetworkFileLink;
}

export interface UserProxyForm {
  username?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  password?: string;
  password_retype?: string;
  keepPassword: boolean;
  isDefault: boolean;
  fileLink?: NetworkFileLink;
}
