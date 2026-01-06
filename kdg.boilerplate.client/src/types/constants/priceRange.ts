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

