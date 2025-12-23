type CategoryBreadcrumb = {
  id: string;
  name: string;
  fullPath: string;
};

export type SubcategoryInfo = {
  id: string;
  name: string;
  fullPath: string;
};

export type CategoryDetail = {
  id: string;
  name: string;
  fullPath: string;
  breadcrumbs: CategoryBreadcrumb[];
  subcategories: SubcategoryInfo[];
};

