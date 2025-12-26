import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Col, Row } from "kdg-react";
import { BaseTemplate } from "../_common/templates/BaseTemplate";
import { getCategoryByPath, getCategoryProducts } from "../../api/categories";
import { TProduct } from "../../types/product/product";
import { PaginatedResponse } from "../../types/common/pagination";
import { usePagination } from "../../hooks/usePagination";
import { useProductFilters } from "../../hooks/useProductFilters";
import { SubcategoryNav } from "./components/SubcategoryNav";
import { ProductGrid } from "./components/ProductGrid";
import { FilterSidebar } from "./components/FilterSidebar";
import { CategoryPageSkeleton } from "./components/CategoryPageSkeleton";
import { ROUTE_BASE } from "../../routing/AppRouter";
import { CategoryDetail } from "../../types/category/category";

export const CategoryPage = () => {
  const params = useParams();
  const path = params['*'] || '';

  const [category, setCategory] = useState<CategoryDetail | null>(null);
  const [products, setProducts] = useState<PaginatedResponse<TProduct> | null>(null);
  const [loading, setLoading] = useState(true);
  const [productsLoading, setProductsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { pagination, setPage } = usePagination();
  const { filters, selectedPriceRange, setPriceRange } = useProductFilters();

  // Load category details
  useEffect(() => {
    if (!path) {
      setError('No category path provided');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    getCategoryByPath({
      path,
      success: (data) => {
        setCategory(data);
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
  }, [path]);

  // Load products when category, pagination, or filters change
  useEffect(() => {
    if (!category) return;

    setProductsLoading(true);

    getCategoryProducts({
      categoryId: category.id,
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
  }, [category, pagination.page, pagination.pageSize, filters.minPrice, filters.maxPrice]);

  if (loading) {
    return (
      <BaseTemplate>
        <CategoryPageSkeleton />
      </BaseTemplate>
    );
  }

  if (error || !category) {
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

  return (
    <BaseTemplate>
      <Row>
        <Col md={12}>
          {/* Breadcrumbs */}
          <nav aria-label="breadcrumb" className="mb-4">
            <ol className="breadcrumb">
              <li className="breadcrumb-item">
                <Link to="/">Home</Link>
              </li>
              {category.breadcrumbs.map((crumb, idx) => (
                <li 
                  key={crumb.id} 
                  className={`breadcrumb-item ${idx === category.breadcrumbs.length - 1 ? 'active' : ''}`}
                  aria-current={idx === category.breadcrumbs.length - 1 ? 'page' : undefined}
                >
                  {idx === category.breadcrumbs.length - 1 ? (
                    crumb.name
                  ) : (
                    <Link to={`${ROUTE_BASE.Categories}/${crumb.fullPath}`}>{crumb.name}</Link>
                  )}
                </li>
              ))}
            </ol>
          </nav>

          {/* Category Title */}
          <h1 className="mb-4">{category.name}</h1>

          {/* Subcategory Navigation */}
          <SubcategoryNav subcategories={category.subcategories} />

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

