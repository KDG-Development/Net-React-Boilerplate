export type PaginatedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export const PAGINATION_PARAM_KEYS = {
  page: 'page',
  pageSize: 'pageSize',
} as const;

export type PaginationParams = {
  [K in keyof typeof PAGINATION_PARAM_KEYS]: number;
};

