import type { NetworkUser } from "./networkUser";

export interface PageBlock {
  id: string;
  parentPageId?: string;
  text: string;
  position: number;
  data: object;
  customPageId: string;
  author: NetworkUser;
}

export interface CreatePageBlock {
  text: string;
  position: number;
  data: object;
  customPageId: string;
}

export interface TreeNode {
  id: string
  text: string
  parentPageId?: string
  children: TreeNode[]
  recursive: boolean
  visited: boolean
}
