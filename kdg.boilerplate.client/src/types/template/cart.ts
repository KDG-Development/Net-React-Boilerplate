import { TProduct } from "../product/product";

export type TCartItem = TProduct & {
  quantity: number;
};

