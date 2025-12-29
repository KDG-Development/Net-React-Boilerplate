import { useState } from "react";
import { ActionButton, Enums, Icon, Image } from "kdg-react";
import { Drawer } from "../../../../components/Drawer";
import { useCartContext } from "../../../../context/CartContext";

export const CartWidget = () => {
  const [isOpen, setIsOpen] = useState(false);
  const { cartItems, updateQuantity, removeItem, subtotal, itemCount } = useCartContext();

  return (
    <>
      <div className="position-relative ms-3" onClick={() => setIsOpen(true)} role="button">
        <Icon
          icon={(x) => x.cilCart}
          size="xl"
        />
        {itemCount > 0 && (
          <span
            className="position-absolute top-0 start-100 translate-middle badge rounded-circle bg-danger d-flex align-items-center justify-content-center"
            style={{ width: "18px", height: "18px", fontSize: "0.65rem" }}
          >
            {itemCount > 99 ? "99+" : itemCount}
          </span>
        )}
      </div>
      <Drawer
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
      header={() => (
        <div className="d-flex justify-content-between align-items-center flex-grow-1 me-2">
          <h5 className="mb-0">Shopping Cart</h5>
          <span className="badge bg-primary">{itemCount} items</span>
        </div>
      )}
      footer={() => (
        <div className="w-100">
          <div className="d-flex justify-content-between mb-3">
            <span className="fw-bold">Subtotal:</span>
            <span className="fw-bold">${subtotal.toFixed(2)}</span>
          </div>
          <div className="d-grid gap-2">
            <ActionButton onClick={() => {}} color={Enums.Color.Primary}>
              Checkout
            </ActionButton>
            <ActionButton onClick={() => setIsOpen(false)} variant="outline">
              Continue Shopping
            </ActionButton>
          </div>
        </div>
      )}
    >
      <div className="d-flex flex-column gap-3">
        {cartItems.map((item) => (
          <div key={item.id} className="d-flex gap-3 border-bottom pb-3">
            <Image src={item.image?.src ?? ""} width={80} height={80} className="rounded" />
            <div className="flex-grow-1">
              <h6 className="mb-1">{item.name}</h6>
              <div className="d-flex align-items-center gap-2 my-1">
                <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => updateQuantity(item.id, -1)}>-</button>
                <span className="small">{item.quantity}</span>
                <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => updateQuantity(item.id, 1)}>+</button>
              </div>
              <div className="fw-bold">${(item.price * item.quantity).toFixed(2)}</div>
            </div>
            <button type="button" className="btn-close align-self-start" aria-label="Remove" onClick={() => removeItem(item.id)} />
          </div>
        ))}
      </div>
    </Drawer>
    </>
  );
};

