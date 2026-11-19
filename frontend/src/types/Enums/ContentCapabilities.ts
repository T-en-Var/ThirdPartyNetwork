/** [Flags] enum in source — values may be OR-combined. */
export enum ContentCapabilities {
  None = 0,
  View = 1 << 0,
  Edit = 1 << 1,
  Share = 1 << 2,
  Comment = 1 << 3,
  Like = 1 << 4,
}
