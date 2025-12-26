import { useEffect, useState, useCallback, useMemo } from "react";
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

  const categoriesWithAllProducts = useMemo(() => {
    if (!categories) return null;
    return {
      __all__: { label: "All Products", fullPath: "" },
      ...categories,
    };
  }, [categories]);

  const handleSelect = useCallback((fullPath: string) => {
    navigate(fullPath ? `${ROUTE_BASE.Products}/${fullPath}` : ROUTE_BASE.Products);
    props.onCategorySelect?.(fullPath);
  }, [navigate, props.onCategorySelect]);

  return (
    <MegaMenu
      trigger={<span>Products</span>}
      categories={categoriesWithAllProducts}
      loading={loading}
      onSelect={handleSelect}
    />
  );
};

