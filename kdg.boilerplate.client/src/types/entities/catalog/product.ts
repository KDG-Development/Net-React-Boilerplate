export type TProductImage = {
  id: string;
  productId: string;
  src: string;
  sortOrder: number;
};

export type TProductBreadcrumb = {
  id: string;
  name: string;
  slug: string;
};

// Core product properties shared across all product types
type TProductCore = {
  id: string;
  name: string;
  description: string | null;
  price: number;
};

// Lightweight product reference with single image (e.g., cart items)
export type TProductMeta = TProductCore & {
  image: TProductImage | null;
};

// Full product with category and multiple images
type TProductFull = TProductCore & {
  categoryId: string | null;
  images: TProductImage[];
};

// Catalog product base (user-facing with favorite status)
type TCatalogProduct = TProductFull & {
  isFavorite: boolean;
};

// Product summary for catalog listings
export type TCatalogProductSummary = TCatalogProduct;

// Product detail for catalog product pages
export type TCatalogProductDetail = TCatalogProduct & {
  breadcrumbs: TProductBreadcrumb[];
};

