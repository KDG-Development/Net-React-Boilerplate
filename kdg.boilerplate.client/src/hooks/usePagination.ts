import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { PaginationParams } from '../types/common/pagination';

const DEFAULT_PAGE = 1;
const DEFAULT_PAGE_SIZE = 20;

export const usePagination = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const pagination: PaginationParams = useMemo(() => ({
    page: parseInt(searchParams.get('page') ?? '', 10) || DEFAULT_PAGE,
    pageSize: parseInt(searchParams.get('pageSize') ?? '', 10) || DEFAULT_PAGE_SIZE,
  }), [searchParams]);

  const setPage = useCallback((page: number) => {
    const newParams = new URLSearchParams(searchParams);
    if (page === DEFAULT_PAGE) {
      newParams.delete('page');
    } else {
      newParams.set('page', String(page));
    }
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  const setPageSize = useCallback((pageSize: number) => {
    const newParams = new URLSearchParams(searchParams);
    newParams.delete('page'); // Reset to page 1
    if (pageSize === DEFAULT_PAGE_SIZE) {
      newParams.delete('pageSize');
    } else {
      newParams.set('pageSize', String(pageSize));
    }
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  return { pagination, setPage, setPageSize };
};

