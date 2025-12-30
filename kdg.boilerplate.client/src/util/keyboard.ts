import { KeyboardEvent } from "react";

export const onEnterKey = (callback: () => void) => (e: KeyboardEvent) => {
  if (e.key === "Enter") {
    callback();
  }
};

