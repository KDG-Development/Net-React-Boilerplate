import { Api } from "./_common"
import { TCartItem } from "../types/template/cart"

export type TCartItemRequest = {
  productId: string;
  quantity: number;
};

const mapCartResponse = (data: any): TCartItem[] =>
  data.map((item: any): TCartItem => ({
    id: item.product.id,
    name: item.product.name,
    description: item.product.description,
    price: item.product.price,
    quantity: item.quantity,
    image: item.product.image,
  }));

export const getCart = async (args: { 
  success: (cart: TCartItem[]) => void;
  errorHandler?: (e: any) => void;
}) => {
  await Api.Request.Get({
    url: `${Api.BASE_URL}/cart`,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: mapCartResponse,
  });
};

export const replaceCart = async (args: {
  items: TCartItemRequest[];
  success: (cart: TCartItem[]) => void;
  errorHandler?: (e: any) => void;
}) => {
  await Api.Request.Post({
    url: `${Api.BASE_URL}/cart`,
    body: args.items,
    success: args.success,
    errorHandler: args.errorHandler,
    mapResult: mapCartResponse,
  });
};
