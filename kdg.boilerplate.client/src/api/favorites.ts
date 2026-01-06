import { RequestMethodArgs, PostRequestMethodArgs } from "kdg-react"
import { Api } from "./_common"

type TAddFavoriteRequest = { productId: string }

export const addFavorite = async (args: PostRequestMethodArgs<TAddFavoriteRequest, {}>) => {
  await Api.Request.Post({
    url: `${Api.BASE_URL}/favorites/${args.body.productId}`,
    body: {},
    success: args.success,
    errorHandler: args.errorHandler,
  });
};

export const removeFavorite = async (args: RequestMethodArgs<{}> & { productId: string }) => {
  await Api.Request.Delete({
    url: `${Api.BASE_URL}/favorites/${args.productId}`,
    success: args.success,
    errorHandler: args.errorHandler,
  });
};

