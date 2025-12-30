import type { NetworkFile } from "./files";
import type { NetworkUser } from "./networkUser";

export interface Blog {
  id: string;
  title: string;
  slug: string;
  summary: string;
  body: object;
  networkId: string;
  author: NetworkUser;
  coverImage?: NetworkFile;
  authorId: string;
  accessLevel: number;
  publishedAt?: Date;
}

export interface CreateBlog {
  title: string;
  summary: string;
  body: object;
  publishedAt?: Date;
  coverImageId?: string;
  accessLevel: number;
}