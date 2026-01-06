import { TProductMeta } from "../catalog/product";

export type TCartItem = TProductMeta & {
  quantity: number;
};

