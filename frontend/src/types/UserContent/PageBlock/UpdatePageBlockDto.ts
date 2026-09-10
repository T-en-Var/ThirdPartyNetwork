export interface UpdatePageBlockDto {
  parentPageId: string | null;
  text: string;
  position: number;
  data: unknown;
}
