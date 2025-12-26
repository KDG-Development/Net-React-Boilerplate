import { RequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { PaginatedResponse, PaginationParams } from "../types/common/pagination"
import { TProduct, TProductImage, TProductDetail, TProductBreadcrumb, ProductFilterParams } from "../types/product/product"

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

export const getProductById = async (
  args: RequestMethodArgs<TProductDetail> & { productId: string }
) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/products/${args.productId}`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (data: any): TProductDetail => ({
      id: data.id,
      name: data.name,
      images: (data.images).map((i: any): TProductImage => ({
        id: i.id,
        productId: i.productId,
        src: i.src,
        sortOrder: i.sortOrder,
      })),
      description: data.description,
      price: data.price,
      breadcrumbs: (data.breadcrumbs).map((b: any): TProductBreadcrumb => ({
        id: b.id,
        name: b.name,
        slug: b.slug,
      })),
    }),
  });
}

