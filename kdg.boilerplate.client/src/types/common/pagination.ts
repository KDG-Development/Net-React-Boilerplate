export type PaginatedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type PaginationParams = {
  page: number;
  pageSize: number;
};

