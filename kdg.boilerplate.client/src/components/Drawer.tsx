import { ReactNode } from "react";
import { Icon } from "kdg-react";

type DrawerProps = {
  isOpen: boolean;
  onClose: () => void;
  header?: () => ReactNode;
  children: ReactNode;
  footer?: () => ReactNode;
  position?: "start" | "end";
  width?: string;
};

export const Drawer = (props: DrawerProps) => {
  const position = props.position ?? "end";
  return (
    <>
      <div
        className={`offcanvas offcanvas-${position} ${props.isOpen ? "show" : ""}`}
        style={props.width ? { width: props.width } : undefined}
        tabIndex={-1}
        aria-modal="true"
        role="dialog"
      >
        <div className="offcanvas-header border-bottom">
          {props.header && props.header()}
          <Icon
            icon={(x) => x.cilX}
            onClick={props.onClose}
            className="cursor-pointer"
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

