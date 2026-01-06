import { RequestMethodArgs, PostRequestMethodArgs } from "kdg-react"
import { Api } from "../_common"
import { THeroSlide } from "../../types/crm/heroSlide"
import { THeroSlideFilters, TCreateHeroSlideDto, TUpdateHeroSlideDto, TReorderHeroSlidesDto } from "../../types/requests/heroSlides/heroSlides"

export const getHeroSlides = async (args: RequestMethodArgs<THeroSlide[]> & { filters?: THeroSlideFilters }) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/heroslides`,
    parameters: Api.composeQueryParams({
      isActive: args.filters?.isActive,
    }),
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

export const getHeroSlide = async (args: RequestMethodArgs<THeroSlide> & { id: string }) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/heroslides/${args.id}`,
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

export const createHeroSlide = async (args: PostRequestMethodArgs<TCreateHeroSlideDto, THeroSlide>) => {
  await Api.Request.PostForm({
    url: `${Api.BASE_URL}/heroslides`,
    body: args.body,
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

export const updateHeroSlide = async (args: PostRequestMethodArgs<TUpdateHeroSlideDto, THeroSlide> & { id: string }) => {
  await Api.Request.PutForm({
    url: `${Api.BASE_URL}/heroslides/${args.id}`,
    body: args.body,
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

export const deleteHeroSlide = async (args: RequestMethodArgs<{}> & { id: string }) => {
  await Api.Request.Delete({
    url: `${Api.BASE_URL}/heroslides/${args.id}`,
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

export const reorderHeroSlides = async (args: PostRequestMethodArgs<TReorderHeroSlidesDto, {}>) => {
  await Api.Request.Put({
    url: `${Api.BASE_URL}/heroslides/reorder`,
    body: args.body,
    success: args.success,
    errorHandler: args.errorHandler,
  })
}

