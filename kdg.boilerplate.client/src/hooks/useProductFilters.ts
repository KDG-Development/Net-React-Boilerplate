import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { ProductFilterParams, FILTER_PARAM_KEYS, PRICE_RANGES, PRICE_RANGE_ID, PriceRange, PriceRangeId, DEFAULT_PRICE_RANGE } from '../types/product/product';
import { PAGINATION_PARAM_KEYS } from '../types/common/pagination';

const isPriceRangeId = (value: string | null): value is PriceRangeId => {
  return value !== null && Object.values(PRICE_RANGE_ID).includes(value as PriceRangeId);
};

/** Build URLSearchParams with only search term (resets all other filters) */
export const getResetSearchParams = (search: string | null): URLSearchParams => {
  const params = new URLSearchParams();
  if (search) {
    params.set(FILTER_PARAM_KEYS.search, search);
  }
  return params;
};

export const useProductFilters = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const selectedPriceRange: PriceRange = useMemo(() => {
    const priceRangeParam = searchParams.get(FILTER_PARAM_KEYS.priceRange);
    return isPriceRangeId(priceRangeParam) ? PRICE_RANGES[priceRangeParam] : DEFAULT_PRICE_RANGE;
  }, [searchParams]);

  const search: string | null = useMemo(() => {
    return searchParams.get(FILTER_PARAM_KEYS.search) || null;
  }, [searchParams]);

  const filters: ProductFilterParams = useMemo(() => ({
    minPrice: selectedPriceRange.minPrice,
    maxPrice: selectedPriceRange.maxPrice,
    search,
  }), [selectedPriceRange, search]);

  const setPriceRange = useCallback((range: PriceRange) => {
    const newParams = new URLSearchParams(searchParams);
    
    // Reset to page 1 when filters change
    newParams.delete(PAGINATION_PARAM_KEYS.page);

    if (range.id === PRICE_RANGE_ID.all) {
      newParams.delete(FILTER_PARAM_KEYS.priceRange);
    } else {
      newParams.set(FILTER_PARAM_KEYS.priceRange, range.id);
    }

    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  const setSearch = useCallback((term: string | null) => {
    const newParams = new URLSearchParams(searchParams);
    
    // Reset to page 1 when search changes
    newParams.delete(PAGINATION_PARAM_KEYS.page);

    if (term) {
      newParams.set(FILTER_PARAM_KEYS.search, term);
    } else {
      newParams.delete(FILTER_PARAM_KEYS.search);
    }

    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  const clearFilters = useCallback(() => {
    const newParams = new URLSearchParams(searchParams);
    newParams.delete(FILTER_PARAM_KEYS.priceRange);
    newParams.delete(FILTER_PARAM_KEYS.search);
    newParams.delete(PAGINATION_PARAM_KEYS.page);
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  return { filters, search, selectedPriceRange, setPriceRange, setSearch, clearFilters };
};
