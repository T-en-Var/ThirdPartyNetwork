import type { UserContentBase, UserContentCreateBase } from "./userContentBase";

export interface Configuration extends UserContentBase {
  id: string;
  key: string;
  value: object;
}

export interface CreateConfiguration extends UserContentCreateBase {
  key: string;
  value: object;
}