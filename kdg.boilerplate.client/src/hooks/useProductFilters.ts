import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { ProductFilterParams, FILTER_PARAM_KEYS } from '../types/requests/products/products';
import { PAGINATION_PARAM_KEYS } from '../types/common/pagination';

const parseNumber = (value: string | null): number | null => {
  if (value === null) return null;
  const num = Number(value);
  return isNaN(num) ? null : num;
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

  const filter: ProductFilterParams = useMemo(() => {
    const favoritesOnly = searchParams.get(FILTER_PARAM_KEYS.favoritesOnly) === 'true';
    return {
      minPrice: favoritesOnly ? null : parseNumber(searchParams.get(FILTER_PARAM_KEYS.minPrice)),
      maxPrice: favoritesOnly ? null : parseNumber(searchParams.get(FILTER_PARAM_KEYS.maxPrice)),
      search: favoritesOnly ? null : searchParams.get(FILTER_PARAM_KEYS.search) || null,
      favoritesOnly,
    };
  }, [searchParams]);

  const setFilter = useCallback((newFilter: ProductFilterParams) => {
    // Toggling favoritesOnly on resets other filters
    if (newFilter.favoritesOnly && !filter.favoritesOnly) {
      const newParams = new URLSearchParams();
      const category = searchParams.get('category');
      if (category) {
        newParams.set('category', category);
      }
      newParams.set(FILTER_PARAM_KEYS.favoritesOnly, 'true');
      setSearchParams(newParams, { replace: true });
      return;
    }

    const newParams = new URLSearchParams(searchParams);
    newParams.delete(PAGINATION_PARAM_KEYS.page);

    // Price range
    if (newFilter.minPrice !== null) {
      newParams.set(FILTER_PARAM_KEYS.minPrice, String(newFilter.minPrice));
    } else {
      newParams.delete(FILTER_PARAM_KEYS.minPrice);
    }

    if (newFilter.maxPrice !== null) {
      newParams.set(FILTER_PARAM_KEYS.maxPrice, String(newFilter.maxPrice));
    } else {
      newParams.delete(FILTER_PARAM_KEYS.maxPrice);
    }

    // Search
    if (newFilter.search) {
      newParams.set(FILTER_PARAM_KEYS.search, newFilter.search);
    } else {
      newParams.delete(FILTER_PARAM_KEYS.search);
    }

    // Favorites
    if (newFilter.favoritesOnly) {
      newParams.set(FILTER_PARAM_KEYS.favoritesOnly, 'true');
    } else {
      newParams.delete(FILTER_PARAM_KEYS.favoritesOnly);
    }

    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams, filter.favoritesOnly]);

  const clearFilters = useCallback(() => {
    const newParams = new URLSearchParams(searchParams);
    newParams.delete(FILTER_PARAM_KEYS.minPrice);
    newParams.delete(FILTER_PARAM_KEYS.maxPrice);
    newParams.delete(FILTER_PARAM_KEYS.search);
    newParams.delete(FILTER_PARAM_KEYS.favoritesOnly);
    newParams.delete(PAGINATION_PARAM_KEYS.page);
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  return { filter, setFilter, clearFilters };
};
