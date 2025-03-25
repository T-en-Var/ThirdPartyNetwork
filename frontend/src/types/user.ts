import type { UserProxy } from "./userProxy";

export interface User {
  id: string,
  proxies: UserProxy[],
}