import { createContext, useContext, useState, useEffect, useCallback } from "react";
import { TCatalogProductSummary } from "../types/entities/catalog/product";
import { TCartItem } from "../types/entities/cart/cart";
import { getCart, replaceCart } from "../api/cart";
import { TCartItemRequest } from "../types/requests/cart/cart";
import { useAuthContext } from "./AuthContext";

type TCartContext = {
  cartItems: TCartItem[];
  addItem: (product: TCatalogProductSummary, quantity?: number) => void;
  updateQuantity: (id: string, delta: number) => void;
  removeItem: (id: string) => void;
  clearCart: () => void;
  subtotal: number;
  itemCount: number;
  isLoading: boolean;
};

const CartContext = createContext<TCartContext | undefined>(undefined);

type TProviderProps = {
  children: React.ReactNode;
};

const toCartItemRequests = (items: TCartItem[]): TCartItemRequest[] =>
  items.map((item) => ({
    productId: item.id,
    quantity: item.quantity,
  }));

export const CartContextProvider = (props: TProviderProps) => {
  const [cartItems, setCartItems] = useState<TCartItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const {user} = useAuthContext()

  const persistCart = useCallback((newItems: TCartItemRequest[]) => {
    replaceCart({
      body: newItems,
      success: setCartItems,
    });
  }, []);

  useEffect(() => {
    if (!user) {
      setIsLoading(false);
      return;
    }
    getCart({
      success: (items) => {
        setCartItems(items);
        setIsLoading(false);
      },
      errorHandler: () => {
        setIsLoading(false);
      },
    });
  }, [user]);

  const addItem = (product: TCatalogProductSummary, quantity: number = 1) => {
    const existingItem = cartItems.find((item) => item.id === product.id);
    const newItems = existingItem
      ? cartItems.map((item) =>
          item.id === product.id
            ? { productId: item.id, quantity: item.quantity + quantity }
            : { productId: item.id, quantity: item.quantity }
        )
      : [...toCartItemRequests(cartItems), { productId: product.id, quantity }];
    persistCart(newItems);
  };

  const updateQuantity = (id: string, delta: number) => {
    const newItems = cartItems
      .map((item) => ({
        productId: item.id,
        quantity: item.id === id ? item.quantity + delta : item.quantity,
      }))
      .filter((item) => item.quantity > 0);
    persistCart(newItems);
  };

  const removeItem = (id: string) => {
    const newItems = cartItems
      .filter((item) => item.id !== id)
      .map((item) => ({ productId: item.id, quantity: item.quantity }));
    persistCart(newItems);
  };

  const clearCart = () => {
    persistCart([]);
  };

  const subtotal = cartItems.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  const itemCount = cartItems.reduce((sum, item) => sum + item.quantity, 0);

  return (
    <CartContext.Provider
      value={{
        cartItems,
        addItem,
        updateQuantity,
        removeItem,
        clearCart,
        subtotal,
        itemCount,
        isLoading,
      }}
    >
      {props.children}
    </CartContext.Provider>
  );
};

export const useCartContext = () => {
  const cartContext = useContext(CartContext);
  if (!cartContext)
    throw new Error("useCartContext called outside context provider");

  return cartContext;
};
