import type { NetworkUser } from "./networkUser";
import type { PageBlock } from "./pageBlock";

export interface CustomPage {
  id: string;
  name: string;
  slug: string;
  networkId: string;
  author: NetworkUser;
  pages: PageBlock[]
}

export interface CreateCustomPage {
  name: string;
  slug: string;
}