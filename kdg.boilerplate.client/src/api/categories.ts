import { RequestMethodArgs } from "kdg-react"
import { CategoryTree, CategoryNode } from "../views/_common/templates/components/MegaMenu"
import { Api } from "./_common"
import { PaginatedResponse, PaginationParams } from "../types/common/pagination"
import { TProduct } from "../types/product/product"
import { CategoryDetail } from "../types/category/category"

export const getCategories = async (args: RequestMethodArgs<CategoryTree>) => {
  const mapCategory = (category: any): CategoryNode => {
    return {
      label: category.label,
      fullPath: category.fullPath,
      children: category.children
        ? Object.fromEntries(
            Object.entries(category.children).map(([key, child]) => [key, mapCategory(child)])
          )
        : undefined,
    };
  };

  await Api.Request.Get({
    url: `${Api.BASE_URL}/categories`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (response: unknown): CategoryTree =>
      Object.fromEntries(
        Object.entries(response as Record<string, unknown>).map(([key, category]) => [key, mapCategory(category)])
      )
  })
}

export const getCategoryByPath = async (args: RequestMethodArgs<CategoryDetail> & { path: string }) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/categories/by-path?path=${encodeURIComponent(args.path)}`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (response: unknown): CategoryDetail => {
      const data = response as Record<string, unknown>;
      return {
        id: data.id as string,
        name: data.name as string,
        fullPath: data.fullPath as string,
        breadcrumbs: (data.breadcrumbs as Array<Record<string, unknown>>).map(b => ({
          id: b.id as string,
          name: b.name as string,
          fullPath: b.fullPath as string,
        })),
        subcategories: (data.subcategories as Array<Record<string, unknown>>).map(s => ({
          id: s.id as string,
          name: s.name as string,
          fullPath: s.fullPath as string,
        })),
      };
    }
  })
}

export const getCategoryProducts = async (
  args: RequestMethodArgs<PaginatedResponse<TProduct>> & { 
    categoryId: string; 
    pagination: PaginationParams;
  }
) => {
  const queryParams = new URLSearchParams({
    page: String(args.pagination.page),
    pageSize: String(args.pagination.pageSize),
  });

  await Api.Request.Get({
    url: `${Api.BASE_URL}/categories/${args.categoryId}/products?${queryParams}`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: (response: unknown): PaginatedResponse<TProduct> => {
      const data = response as Record<string, unknown>;
      return {
        items: (data.items as Array<Record<string, unknown>>).map(p => ({
          id: p.id as string,
          name: p.name as string,
          image: '', // No image field in DB yet
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
