import { RequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { PaginatedResponse, PaginationParams } from "../types/common/pagination"
import { TProduct, ProductFilterParams } from "../types/product/product"

export const getProducts = async (
  args: RequestMethodArgs<PaginatedResponse<TProduct>> & { 
    categoryId?: string;
    pagination: PaginationParams;
    filters?: ProductFilterParams;
  }
) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/products`,
    parameters: Api.composeQueryParams({
      categoryId: args.categoryId,
      page: args.pagination.page,
      pageSize: args.pagination.pageSize,
      minPrice: args.filters?.minPrice,
      maxPrice: args.filters?.maxPrice,
      search: args.filters?.search,
    }),
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (response: unknown): PaginatedResponse<TProduct> => {
      const data = response as Record<string, unknown>;
      return {
        items: (data.items as Array<Record<string, unknown>>).map(p => ({
          id: p.id as string,
          name: p.name as string,
          image: '', // No image field in DB yet
          description: (p.description as string) ?? null,
          price: p.price as number,
        })),
        page: data.page as number,
        pageSize: data.pageSize as number,
        totalCount: data.totalCount as number,
        totalPages: data.totalPages as number,
      };
    }
  })
}

