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

