import { Api } from "./_common"

export const addFavorite = async (args: {
  productId: string;
  success: () => void;
  errorHandler?: (e: any) => void;
}) => {
  await Api.Request.Post({
    url: `${Api.BASE_URL}/favorites/${args.productId}`,
    body: {},
    success: args.success,
    errorHandler: args.errorHandler,
  });
};

export const removeFavorite = async (args: {
  productId: string;
  success: () => void;
  errorHandler?: (e: any) => void;
}) => {
  await Api.Request.Delete({
    url: `${Api.BASE_URL}/favorites/${args.productId}`,
    success: args.success,
    errorHandler: args.errorHandler,
  });
};

