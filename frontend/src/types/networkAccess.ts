import type { Access } from "./access";
import type { Network } from "./network";

export interface NetworkAccess {
  networkId: string,
  accessId: string,
  isRequired: boolean,
  access: Access,
  network: Network,
}
