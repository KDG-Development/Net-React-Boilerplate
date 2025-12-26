import { useEffect, useState, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { getCategories } from "../../../../api/categories";
import { MegaMenu, CategoryTree } from "./MegaMenu";
import { ROUTE_PATH } from "../../../../routing/AppRouter";

type CategoryMegaMenuProps = {
  onCategorySelect?: (slug: string) => void;
  variant?: "desktop" | "mobile";
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
      __all__: { label: "All Products", slug: "" },
      ...categories,
    };
  }, [categories]);

  const handleSelect = useCallback((slug: string) => {
    navigate(slug ? `${ROUTE_PATH.Products}?category=${slug}` : ROUTE_PATH.Products);
    props.onCategorySelect?.(slug);
  }, [navigate, props.onCategorySelect]);

  return (
    <MegaMenu
      trigger={<span>Products</span>}
      categories={categoriesWithAllProducts}
      loading={loading}
      onSelect={handleSelect}
      variant={props.variant}
    />
  );
};

