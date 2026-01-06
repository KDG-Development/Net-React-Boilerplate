import { useState } from "react";
import { Clickable } from "kdg-react";
import { addFavorite, removeFavorite } from "../../api/favorites";
import { useAuthContext } from "../../context/AuthContext";
import { HeartIcon } from "./HeartIcon";
import "./FavoriteToggle.css";

type FavoriteToggleProps = {
  productId: string;
  isFavorite: boolean;
  className?: string;
  size?: number;
};

export const FavoriteToggle = (props: FavoriteToggleProps) => {
  const { user } = useAuthContext();
  const [isFavorite, setIsFavorite] = useState(props.isFavorite);

  if (!user?.user.organization) {
    return null;
  }

  const btnClass = isFavorite ? "is-favorite" : "is-not-favorite";

  return (
    <Clickable
      className={`favorite-toggle-btn m-1 ${btnClass} ${props.className ?? ""}`}
      onClick={() => {
        if (isFavorite) {
          removeFavorite({
            productId: props.productId,
            success: () => {
              setIsFavorite(false);
            },
          });
        } else {
          addFavorite({
            body: { productId: props.productId },
            success: () => {
              setIsFavorite(true);
            },
          });
        }
      }}
    >
      <HeartIcon
        filled={isFavorite}
        className="favorite-icon"
        size={props.size ?? 24}
      />
    </Clickable>
  );
};
