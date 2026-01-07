import type { UserContentBase } from "./userContentBase";

export interface PageBlock extends UserContentBase {
  id: string;
  parentPageId?: string;
  text: string;
  position: number;
  data: object;
  customPageId: string;
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
