import type { NetworkUser } from "./networkUser";
import type { Role } from "./role";

export interface NetworkUserRole {
  networkUser: NetworkUser,
  role: Role
}