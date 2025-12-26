import { useState, useCallback } from "react";

type TMicroToastItem = {
  id: string;
  message: string;
};

type TShowToast = (message: string) => void;

type TMicroToastProps = {
  children: (show: TShowToast) => React.ReactNode;
  className?: string;
};

const TOAST_DURATION_MS = 1500;

export const MicroToast = (props: TMicroToastProps) => {
  const [toasts, setToasts] = useState<TMicroToastItem[]>([]);

  const show = useCallback((message: string) => {
    const id = crypto.randomUUID();
    setToasts((prev) => [...prev, { id, message }]);

    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, TOAST_DURATION_MS);
  }, []);

  return (
    <div className={`micro-toast-container ${props.className ?? ""}`}>
      {props.children(show)}
      {toasts.map((toast) => (
        <div key={toast.id} className="micro-toast">
          {toast.message}
        </div>
      ))}
    </div>
  );
};

