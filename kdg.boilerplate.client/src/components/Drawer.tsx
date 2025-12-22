import { ReactNode } from "react";

type DrawerProps = {
  isOpen: boolean;
  onClose: () => void;
  header?: () => ReactNode;
  children: ReactNode;
  footer?: () => ReactNode;
};

export const Drawer = (props: DrawerProps) => {
  return (
    <>
      <div
        className={`offcanvas offcanvas-end ${props.isOpen ? "show" : ""}`}
        tabIndex={-1}
        aria-modal="true"
        role="dialog"
      >
        <div className="offcanvas-header border-bottom">
          {props.header && props.header()}
          <button
            type="button"
            className="btn-close"
            aria-label="Close"
            onClick={props.onClose}
          />
        </div>
        <div className="offcanvas-body overflow-auto">{props.children}</div>
        {props.footer && (
          <div className="offcanvas-footer border-top p-3">{props.footer()}</div>
        )}
      </div>
      {props.isOpen && (
        <div className="offcanvas-backdrop fade show" onClick={props.onClose} />
      )}
    </>
  );
};

