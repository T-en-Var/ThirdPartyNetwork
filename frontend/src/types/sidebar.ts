export interface SubItem {
  page: string;
  label: string;
}

export interface NavItem {
  page: string;
  label: string;
  subItems: SubItem[];
  getURI: (sub: SubItem) => string;
}

export interface NavCategory {
  title: string;
  items: NavItem[];
}
