import type { PageBlock } from "./pageBlock";
import type { UserContentBase, UserContentCreateBase } from "./userContentBase";

export interface CustomPage extends UserContentBase {
  id: string;
  name: string;
  slug: string;
  pages: PageBlock[];
}

export interface CreateCustomPage extends UserContentCreateBase {
  name: string;
  slug: string;
}