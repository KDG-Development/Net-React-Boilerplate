import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { PaginationParams, PAGINATION_PARAM_KEYS } from '../types/common/pagination';

const DEFAULT_PAGE = 1;
const DEFAULT_PAGE_SIZE = 20;

export const usePagination = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const pagination: PaginationParams = useMemo(() => ({
    page: parseInt(searchParams.get(PAGINATION_PARAM_KEYS.page) ?? '', 10) || DEFAULT_PAGE,
    pageSize: parseInt(searchParams.get(PAGINATION_PARAM_KEYS.pageSize) ?? '', 10) || DEFAULT_PAGE_SIZE,
  }), [searchParams]);

  const setPage = useCallback((page: number) => {
    const newParams = new URLSearchParams(searchParams);
    if (page === DEFAULT_PAGE) {
      newParams.delete(PAGINATION_PARAM_KEYS.page);
    } else {
      newParams.set(PAGINATION_PARAM_KEYS.page, String(page));
    }
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  const setPageSize = useCallback((pageSize: number) => {
    const newParams = new URLSearchParams(searchParams);
    newParams.delete(PAGINATION_PARAM_KEYS.page); // Reset to page 1
    if (pageSize === DEFAULT_PAGE_SIZE) {
      newParams.delete(PAGINATION_PARAM_KEYS.pageSize);
    } else {
      newParams.set(PAGINATION_PARAM_KEYS.pageSize, String(pageSize));
    }
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  return { pagination, setPage, setPageSize };
};

