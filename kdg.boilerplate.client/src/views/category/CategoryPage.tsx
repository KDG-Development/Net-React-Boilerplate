import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { Col, Row, Clickable, Icon, Conditional, EntityConditional } from "kdg-react";
import { BaseTemplate } from "../_common/templates/BaseTemplate";
import { Drawer } from "../../components/Drawer";
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
  const [filterDrawerOpen, setFilterDrawerOpen] = useState(false);

  const { pagination, setPage } = usePagination();
  const { filters, search, selectedPriceRange, setPriceRange, setSearch } = useProductFilters();

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
  }, [isRootView, category, loading, pagination.page, pagination.pageSize, filters.minPrice, filters.maxPrice, filters.search]);

  const pageTitle = isRootView ? 'All Products' : category?.name || '';
  const subcategories = isRootView ? topLevelCategories : (category?.subcategories || []);

  return (
    <BaseTemplate>
      <Conditional
        condition={loading}
        onTrue={() => <CategoryPageSkeleton />}
        onFalse={() => (
          <Conditional
            condition={!!(error || (!isRootView && !category))}
            onTrue={() => (
              <Row>
                <Col md={12}>
                  <div className="text-center py-5">
                    <h4 className="text-muted">{error || 'Category not found'}</h4>
                    <Link to={ROUTE_PATH.Products} className="btn btn-primary mt-3">
                      Browse Products
                    </Link>
                  </div>
                </Col>
              </Row>
            )}
            onFalse={() => (
              <Row>
                <Col md={12}>
                  {/* Breadcrumbs */}
                  <nav aria-label="breadcrumb" className="mb-4">
                    <ol className="breadcrumb">
                      <Conditional
                        condition={isRootView}
                        onTrue={() => (
                          <li className="breadcrumb-item active" aria-current="page">
                            Products
                          </li>
                        )}
                        onFalse={() => (
                          <EntityConditional
                            entity={category}
                            render={cat => (
                              <>
                                <li className="breadcrumb-item">
                                  <Link to={ROUTE_PATH.Products}>Products</Link>
                                </li>
                                {cat.breadcrumbs.map((crumb, idx) => (
                                  <li 
                                    key={crumb.id} 
                                    className={`breadcrumb-item ${idx === cat.breadcrumbs.length - 1 ? 'active' : ''}`}
                                    aria-current={idx === cat.breadcrumbs.length - 1 ? 'page' : undefined}
                                  >
                                    <Conditional
                                      condition={idx === cat.breadcrumbs.length - 1}
                                      onTrue={() => <>{crumb.name}</>}
                                      onFalse={() => (
                                        <Link to={`${ROUTE_PATH.Products}?category=${crumb.slug}`}>{crumb.name}</Link>
                                      )}
                                    />
                                  </li>
                                ))}
                              </>
                            )}
                          />
                        )}
                      />
                    </ol>
                  </nav>

                  {/* Page Title */}
                  <h1 className="mb-4">{pageTitle}</h1>

                  {/* Subcategory Navigation */}
                  <SubcategoryNav subcategories={subcategories} />

                  {/* Mobile filter button */}
                  <div className="d-lg-none mb-3">
                    <Clickable onClick={() => setFilterDrawerOpen(true)} className="btn btn-outline-secondary">
                      <Icon icon={(x) => x.cilFilter} className="me-2" />
                      Filters
                    </Clickable>
                  </div>

                  {/* Mobile filter drawer */}
                  <div className="d-lg-none">
                    <Drawer
                      isOpen={filterDrawerOpen}
                      onClose={() => setFilterDrawerOpen(false)}
                      header={() => <span className="fw-bold">Filters</span>}
                      position="start"
                    >
                      <FilterSidebar
                        selectedPriceRange={selectedPriceRange}
                        onPriceRangeChange={setPriceRange}
                        search={search}
                        onSearchChange={(term) => {
                          setSearch(term);
                          setFilterDrawerOpen(false);
                        }}
                      />
                    </Drawer>
                  </div>

                  <Row>
                    {/* Filter Sidebar - Desktop only */}
                    <Col md={3} className="d-none d-lg-block">
                      <FilterSidebar
                        selectedPriceRange={selectedPriceRange}
                        onPriceRangeChange={setPriceRange}
                        search={search}
                        onSearchChange={setSearch}
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
            )}
          />
        )}
      />
    </BaseTemplate>
  );
};
