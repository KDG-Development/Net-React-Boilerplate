import { RequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { PaginatedResponse, PaginationParams } from "../types/common/pagination"
import { TCatalogProductSummary, TProductImage, TCatalogProductDetail, TProductBreadcrumb } from "../types/entities/catalog/product"
import { ProductFilterParams } from "../types/requests/products/products"

export const getProducts = async (
  args: RequestMethodArgs<PaginatedResponse<TCatalogProductSummary>> & { 
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
      favoritesOnly: args.filters?.favoritesOnly,
    }),
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (data: any): PaginatedResponse<TCatalogProductSummary> => {
      return {
        items: data.items.map((p: any): TCatalogProductSummary => ({
          id: p.id,
          name: p.name,
          description: p.description,
          price: p.price,
          categoryId: p.categoryId,
          images: (p.images).map((i:any):TProductImage => ({
            id: i.id,
            productId: i.productId,
            src: i.src,
            sortOrder: i.sortOrder,
          })),
          isFavorite: p.isFavorite,
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
  args: RequestMethodArgs<TCatalogProductDetail> & { productId: string }
) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/products/${args.productId}`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (data: any): TCatalogProductDetail => ({
      id: data.id,
      name: data.name,
      description: data.description,
      price: data.price,
      categoryId: data.categoryId,
      images: (data.images).map((i: any): TProductImage => ({
        id: i.id,
        productId: i.productId,
        src: i.src,
        sortOrder: i.sortOrder,
      })),
      isFavorite: data.isFavorite,
      breadcrumbs: (data.breadcrumbs).map((b: any): TProductBreadcrumb => ({
        id: b.id,
        name: b.name,
        slug: b.slug,
      })),
    }),
  });
}

