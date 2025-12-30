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


export const FILTER_PARAM_KEYS = {
  minPrice: 'minPrice',
  maxPrice: 'maxPrice',
  search: 'search',
  favoritesOnly: 'favoritesOnly',
} as const;

export type ProductFilterParams = {
  minPrice: number | null;
  maxPrice: number | null;
  search: string | null;
  favoritesOnly: boolean;
};

export const DEFAULT_PRODUCT_FILTERS: ProductFilterParams = {
  minPrice: null,
  maxPrice: null,
  search: null,
  favoritesOnly: false,
};

// Default price range options for UI display (can be customized/extended)
export type PriceRangeOption = {
  label: string;
  minPrice: number | null;
  maxPrice: number | null;
};

export const DEFAULT_PRICE_RANGE_OPTIONS: PriceRangeOption[] = [
  { label: 'All Prices', minPrice: null, maxPrice: null },
  { label: 'Under $25', minPrice: null, maxPrice: 25 },
  { label: '$25 to $50', minPrice: 25, maxPrice: 50 },
  { label: '$50 to $100', minPrice: 50, maxPrice: 100 },
  { label: '$100 to $200', minPrice: 100, maxPrice: 200 },
  { label: '$200 & Above', minPrice: 200, maxPrice: null },
];

