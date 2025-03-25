import type { User } from "./user";

export interface UserProxy {
  id: string,
  username: string,
  firstName: string,
  lastName: string,
  email: string,
  // password: string,
  profilePicture: string,
  isDefault: boolean,
  user: User
}