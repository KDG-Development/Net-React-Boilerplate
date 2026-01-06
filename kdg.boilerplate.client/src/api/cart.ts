import { RequestMethodArgs, PostRequestMethodArgs } from "kdg-react"
import { Api } from "./_common"
import { TCartItem } from "../types/entities/cart/cart"
import { TCartItemRequest } from "../types/requests/cart/cart"

const mapCartResponse = (data: any): TCartItem[] =>
  data.map((item: any): TCartItem => ({
    id: item.product.id,
    name: item.product.name,
    description: item.product.description,
    price: item.product.price,
    quantity: item.quantity,
    image: item.product.image,
  }));

export const getCart = async (args: RequestMethodArgs<TCartItem[]>) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/cart`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: mapCartResponse,
  });
};

type TReplaceCartRequest = { items: TCartItemRequest[] };

export const replaceCart = async (args: PostRequestMethodArgs<TCartItemRequest[], TCartItem[]>) => {
  await Api.Request.Post<TReplaceCartRequest, TCartItem[]>({
    url: `${Api.BASE_URL}/cart`,
    body: { items: args.body },
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: mapCartResponse,
  });
};

type TCheckoutRequest = { items: TCartItemRequest[] };

export const checkout = async (args: PostRequestMethodArgs<TCheckoutRequest, {}>) => {
  await Api.Request.Post({
    url: `${Api.BASE_URL}/cart/checkout`,
    body: args.body,
    success: args.success,
    errorHandler: args.errorHandler,
  });
};
