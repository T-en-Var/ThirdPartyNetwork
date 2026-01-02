import type { NetworkFile } from "./files";
import type { UserContentBase, UserContentCreateBase } from "./userContentBase";

export interface Blog extends UserContentBase {
  id: string;
  title: string;
  slug: string;
  summary: string;
  body: object;
  coverImage?: NetworkFile;
  publishedAt?: Date;
}

export interface CreateBlog extends UserContentCreateBase {
  title: string;
  summary: string;
  body: object;
  publishedAt?: Date;
  coverImageId?: string;
}
