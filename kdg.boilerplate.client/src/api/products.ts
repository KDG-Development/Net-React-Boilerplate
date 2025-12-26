import { RequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { PaginatedResponse, PaginationParams } from "../types/common/pagination"
import { TProduct, TProductImage, ProductFilterParams } from "../types/product/product"

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
    mapResult: (data: any): PaginatedResponse<TProduct> => {
      return {
        items: data.items.map((p: any):TProduct => ({
          id: p.id,
          name: p.name,
          images: (p.images).map((i:any):TProductImage => ({
            id: i.id,
            productId: i.productId,
            src: i.src,
            sortOrder: i.sortOrder,
          })),
          description: p.description,
          price: p.price,
        })),
        page: data.page,
        pageSize: data.pageSize,
        totalCount: data.totalCount,
        totalPages: data.totalPages,
      };
    }
  })
}

