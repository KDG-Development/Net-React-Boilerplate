import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { getCategories } from "../../../../api/categories";
import { MegaMenu, CategoryTree } from "./MegaMenu";
import { ROUTE_BASE } from "../../../../routing/AppRouter";

type CategoryMegaMenuProps = {
  onCategorySelect?: (fullPath: string) => void;
};

export const CategoryMegaMenu = (props: CategoryMegaMenuProps) => {
  const navigate = useNavigate();
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

  const handleSelect = useCallback((fullPath: string) => {
    if (fullPath) {
      navigate(`${ROUTE_BASE.Categories}/${fullPath}`);
    }
    props.onCategorySelect?.(fullPath);
  }, [navigate, props.onCategorySelect]);

  return (
    <MegaMenu
      trigger={<span>Products</span>}
      categories={categories}
      loading={loading}
      onSelect={handleSelect}
    />
  );
};

