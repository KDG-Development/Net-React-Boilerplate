import { RequestMethodArgs } from "kdg-react"
import { CategoryTree, CategoryNode } from "../views/_common/templates/components/MegaMenu"
import { Api } from "./_common"

export const getCategories = async (args: RequestMethodArgs<CategoryTree>) => {
  const mapCategory = (category: any): CategoryNode => {
    return {
      label: category.label,
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
