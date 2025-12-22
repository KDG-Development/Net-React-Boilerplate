import { useEffect, useState } from "react";
import { getCategories } from "../../../../api/categories";
import { MegaMenu, MegaMenuCategories } from "./MegaMenu";

type ProductsMegaMenuProps = {
  onCategorySelect?: (path: string[]) => void;
};

export const ProductsMegaMenu = (props: ProductsMegaMenuProps) => {
  const [categories, setCategories] = useState<MegaMenuCategories | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCategories({
      success: (data) => {
        setCategories(data);
        setLoading(false);
      },
    });
  }, []);

  return (
    <MegaMenu
      trigger={<span>Products</span>}
      categories={categories}
      loading={loading}
      onSelect={props.onCategorySelect}
    />
  );
};
