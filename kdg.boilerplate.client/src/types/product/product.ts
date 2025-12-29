export type TProductImage = {
  id: string;
  productId: string;
  src: string;
  sortOrder: number;
};

export type TProduct = {
  id: string;
  name: string;
  images: TProductImage[];
  description: string | null;
  price: number;
};

export type TProductMeta = {
  id: string;
  name: string;
  image: TProductImage | null;
  description: string | null;
  price: number;
};

export type TProductBreadcrumb = {
  id: string;
  name: string;
  slug: string;
};

export type TProductDetail = {
  id: string;
  name: string;
  images: TProductImage[];
  description: string | null;
  price: number;
  breadcrumbs: TProductBreadcrumb[];
};

// todo: maybe come from server/settings?
export const PRICE_RANGE_ID = {
  all: 'all',
  under25: 'under-25',
  from25to50: '25-50',
  from50to100: '50-100',
  from100to200: '100-200',
  over200: '200-plus',
} as const;

export type PriceRangeId = typeof PRICE_RANGE_ID[keyof typeof PRICE_RANGE_ID];

export type PriceRange = {
  id: PriceRangeId;
  label: string;
  minPrice: number | null;
  maxPrice: number | null;
};

export const FILTER_PARAM_KEYS = {
  priceRange: 'priceRange',
  search: 'search',
} as const;

export type ProductFilterParams = {
  minPrice: number | null;
  maxPrice: number | null;
  search: string | null;
};

export const DEFAULT_PRODUCT_FILTERS: ProductFilterParams = {
  minPrice: null,
  maxPrice: null,
  search: null,
};

export const PRICE_RANGES: Record<PriceRangeId, PriceRange> = {
  [PRICE_RANGE_ID.all]: { id: PRICE_RANGE_ID.all, label: 'All Prices', minPrice: null, maxPrice: null },
  [PRICE_RANGE_ID.under25]: { id: PRICE_RANGE_ID.under25, label: 'Under $25', minPrice: null, maxPrice: 25 },
  [PRICE_RANGE_ID.from25to50]: { id: PRICE_RANGE_ID.from25to50, label: '$25 to $50', minPrice: 25, maxPrice: 50 },
  [PRICE_RANGE_ID.from50to100]: { id: PRICE_RANGE_ID.from50to100, label: '$50 to $100', minPrice: 50, maxPrice: 100 },
  [PRICE_RANGE_ID.from100to200]: { id: PRICE_RANGE_ID.from100to200, label: '$100 to $200', minPrice: 100, maxPrice: 200 },
  [PRICE_RANGE_ID.over200]: { id: PRICE_RANGE_ID.over200, label: '$200 & Above', minPrice: 200, maxPrice: null },
};

export const DEFAULT_PRICE_RANGE = PRICE_RANGES[PRICE_RANGE_ID.all];

