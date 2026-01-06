import { RequestMethodArgs } from "kdg-react"
import { CategoryTree, CategoryNode } from "../types/entities/catalog/category"
import { Api } from "./_common"
import { CategoryDetail } from "../types/entities/catalog/category"

export const getCategories = async (args: RequestMethodArgs<CategoryTree>) => {
  const mapCategory = (category: any): CategoryNode => {
    return {
      label: category.label,
      slug: category.slug,
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
        slug: data.slug as string,
        breadcrumbs: (data.breadcrumbs as Array<Record<string, unknown>>).map(b => ({
          id: b.id as string,
          name: b.name as string,
          slug: b.slug as string,
        })),
        subcategories: (data.subcategories as Array<Record<string, unknown>>).map(s => ({
          id: s.id as string,
          name: s.name as string,
          slug: s.slug as string,
        })),
      };
    }
  })
}
