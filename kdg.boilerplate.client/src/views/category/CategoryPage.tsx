import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { Col, Row } from "kdg-react";
import { BaseTemplate } from "../_common/templates/BaseTemplate";
import { getCategoryByPath, getCategories } from "../../api/categories";
import { getProducts } from "../../api/products";
import { CategoryTree } from "../_common/templates/components/MegaMenu";
import { TProduct } from "../../types/product/product";
import { PaginatedResponse } from "../../types/common/pagination";
import { usePagination } from "../../hooks/usePagination";
import { useProductFilters } from "../../hooks/useProductFilters";
import { SubcategoryNav } from "./components/SubcategoryNav";
import { ProductGrid } from "./components/ProductGrid";
import { FilterSidebar } from "./components/FilterSidebar";
import { CategoryPageSkeleton } from "./components/CategoryPageSkeleton";
import { ROUTE_PATH } from "../../routing/AppRouter";
import { CategoryDetail, SubcategoryInfo } from "../../types/category/category";

export const CategoryPage = () => {
  const [searchParams] = useSearchParams();
  const slug = searchParams.get('category') || '';
  const isRootView = !slug;

  const [category, setCategory] = useState<CategoryDetail | null>(null);
  const [topLevelCategories, setTopLevelCategories] = useState<SubcategoryInfo[]>([]);
  const [products, setProducts] = useState<PaginatedResponse<TProduct> | null>(null);
  const [loading, setLoading] = useState(true);
  const [productsLoading, setProductsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { pagination, setPage } = usePagination();
  const { filters, selectedPriceRange, setPriceRange } = useProductFilters();

  // Load category details or top-level categories for root view
  useEffect(() => {
    setLoading(true);
    setError(null);

    if (isRootView) {
      // Root view: fetch top-level categories
      getCategories({
        success: (data: CategoryTree) => {
          const topLevel = Object.entries(data).map(([id, node]) => ({
            id,
            name: node.label,
            slug: node.slug,
          }));
          setTopLevelCategories(topLevel);
          setCategory(null);
          setLoading(false);
        },
        errorHandler: () => {
          setError('Failed to load categories');
          setLoading(false);
        }
      });
    } else {
      // Category view: fetch category by slug
      getCategoryByPath({
        path: slug,
        success: (data) => {
          setCategory(data);
          setTopLevelCategories([]);
          setLoading(false);
        },
        errorHandler: (e) => {
          if (e.status === 404) {
            setError('Category not found');
          } else {
            setError('Failed to load category');
          }
          setLoading(false);
        }
      });
    }
  }, [slug, isRootView]);

  // Load products when view type, category, pagination, or filters change
  useEffect(() => {
    if (loading) return;
    if (!isRootView && !category) return;

    setProductsLoading(true);

    getProducts({
      categoryId: category?.id,
      pagination,
      filters,
      success: (data) => {
        setProducts(data);
        setProductsLoading(false);
      },
      errorHandler: () => {
        setProducts(null);
        setProductsLoading(false);
      }
    });
  }, [isRootView, category, loading, pagination.page, pagination.pageSize, filters.minPrice, filters.maxPrice]);

  if (loading) {
    return (
      <BaseTemplate>
        <CategoryPageSkeleton />
      </BaseTemplate>
    );
  }

  if (error || (!isRootView && !category)) {
    return (
      <BaseTemplate>
        <Row>
          <Col md={12}>
            <div className="text-center py-5">
              <h4 className="text-muted">{error || 'Category not found'}</h4>
              <Link to="/" className="btn btn-primary mt-3">
                Return Home
              </Link>
            </div>
          </Col>
        </Row>
      </BaseTemplate>
    );
  }

  const pageTitle = isRootView ? 'All Products' : category?.name || '';
  const subcategories = isRootView ? topLevelCategories : (category?.subcategories || []);

  return (
    <BaseTemplate>
      <Row>
        <Col md={12}>
          {/* Breadcrumbs */}
          <nav aria-label="breadcrumb" className="mb-4">
            <ol className="breadcrumb">
              {isRootView ? (
                <li className="breadcrumb-item active" aria-current="page">
                  Products
                </li>
              ) : (
                <>
                  <li className="breadcrumb-item">
                    <Link to={ROUTE_PATH.Products}>Products</Link>
                  </li>
                  {category?.breadcrumbs.map((crumb, idx) => (
                    <li 
                      key={crumb.id} 
                      className={`breadcrumb-item ${idx === category.breadcrumbs.length - 1 ? 'active' : ''}`}
                      aria-current={idx === category.breadcrumbs.length - 1 ? 'page' : undefined}
                    >
                      {idx === category.breadcrumbs.length - 1 ? (
                        crumb.name
                      ) : (
                        <Link to={`${ROUTE_PATH.Products}?category=${crumb.slug}`}>{crumb.name}</Link>
                      )}
                    </li>
                  ))}
                </>
              )}
            </ol>
          </nav>

          {/* Page Title */}
          <h1 className="mb-4">{pageTitle}</h1>

          {/* Subcategory Navigation */}
          <SubcategoryNav subcategories={subcategories} />

          <Row>
            {/* Filter Sidebar */}
            <Col md={3}>
              <FilterSidebar
                selectedPriceRange={selectedPriceRange}
                onPriceRangeChange={setPriceRange}
              />
            </Col>

            {/* Products Grid */}
            <Col md={9}>
              <ProductGrid
                data={products}
                loading={productsLoading}
                onPageChange={setPage}
              />
            </Col>
          </Row>
        </Col>
      </Row>
    </BaseTemplate>
  );
};
