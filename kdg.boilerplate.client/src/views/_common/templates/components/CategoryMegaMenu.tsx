import { useEffect, useState } from "react";
import { getCategories } from "../../../../api/categories";
import { MegaMenu, CategoryTree } from "./MegaMenu";

type CategoryMegaMenuProps = {
  onCategorySelect?: (path: string[]) => void;
};

export const CategoryMegaMenu = (props: CategoryMegaMenuProps) => {
  const [categories, setCategories] = useState<CategoryTree | null>(null);
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

