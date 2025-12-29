import { TProductMeta } from "../product/product";

export type TCartItem = TProductMeta & {
  quantity: number;
};
