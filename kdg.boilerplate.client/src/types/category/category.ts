type CategoryBreadcrumb = {
  id: string;
  name: string;
  slug: string;
};

export type SubcategoryInfo = {
  id: string;
  name: string;
  slug: string;
};

export type CategoryDetail = {
  id: string;
  name: string;
  slug: string;
  breadcrumbs: CategoryBreadcrumb[];
  subcategories: SubcategoryInfo[];
};

