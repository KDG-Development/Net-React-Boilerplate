import { useNavigate } from "react-router-dom";
import { ActionButton, Enums } from "kdg-react";
import { ROUTE_PATH } from "../../../routing/AppRouter";
import { SubcategoryInfo } from "../../../types/category/category";

type SubcategoryNavProps = {
  subcategories: SubcategoryInfo[];
};

export const SubcategoryNav = (props: SubcategoryNavProps) => {
  const navigate = useNavigate();

  if (props.subcategories.length === 0) return null;

  return (
    <div className="mb-4 border-bottom border-bottom-light pb-4">
      <h6 className="text-muted mb-3">Browse Subcategories</h6>
      <div className="d-flex flex-wrap gap-2">
        {props.subcategories.map(sub => (
          <ActionButton
            key={sub.id}
            color={Enums.Color.Primary}
            variant="ghost"
            onClick={() => navigate(`${ROUTE_PATH.Products}?category=${sub.slug}`)}
          >
            {sub.name}
          </ActionButton>
        ))}
      </div>
    </div>
  );
};

