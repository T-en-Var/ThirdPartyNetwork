import type { UserContentBase, UserContentCreateBase } from './userContentBase';

export interface Configuration extends UserContentBase {
  key: string;
  value: object;
}

export interface CreateConfiguration extends UserContentCreateBase {
  key: string;
  value: object;
}
