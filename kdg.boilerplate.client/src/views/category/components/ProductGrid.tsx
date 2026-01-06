import { Card, Clickable, Col, Conditional, EntityConditional, Image, Row, useAppNavigation } from "kdg-react";
import { TCatalogProductSummary } from "../../../types/entities/catalog/product";
import { PaginatedResponse } from "../../../types/common/pagination";
import { formatCurrency } from "../../../util/format";
import { Pagination } from "./Pagination";
import { ProductGridSkeleton } from "./ProductGridSkeleton";
import { ROUTE_BASE } from "../../../routing/AppRouter";
import { FavoriteToggle } from "../../../components/FavoriteToggle/FavoriteToggle";
import placeholderImage from "../../../assets/images/logo.png";

type ProductGridProps = {
  data: PaginatedResponse<TCatalogProductSummary> | null;
  loading: boolean;
  onPageChange: (page: number) => void;
};

type ProductCardProps = {
  product: TCatalogProductSummary;
};

const ProductCard = (props: ProductCardProps) => {
  const primaryImage = props.product.images[0]?.src ?? placeholderImage;
  const navigate = useAppNavigation();
  return (
    <Card
      className="shadow-none h-100 product-card"
      body={{
        content: (
          <div className="position-relative">
            <Clickable onClick={() => navigate(`${ROUTE_BASE.Products}/${props.product.id}`)}>
              <div className="mb-3 d-flex align-items-center justify-content-center product-image-container">
                <Image 
                  src={primaryImage} 
                  className="img-fluid product-image"
                />
              </div>
              <div className="product-info">
                <p className="product-title small fw-semibold mb-1 text-truncate">{props.product.name}</p>
                <p className="product-description text-muted small mb-2 text-truncate">
                  {props.product.description}
                </p>
                <p className="product-price text-primary fw-bold mb-0">
                  {formatCurrency(props.product.price)}
                </p>
              </div>
            </Clickable>
            <FavoriteToggle
              productId={props.product.id}
              isFavorite={props.product.isFavorite}
              className="position-absolute top-0 end-0 p-2"
            />
          </div>
        ),
      }}
    />
  );
};

export const ProductGrid = (props: ProductGridProps) => {
  return (
    <Conditional
      condition={props.loading}
      onTrue={() => (
        <ProductGridSkeleton />
      )}
      onFalse={() => (
        <EntityConditional
          entity={props.data}
          render={data => (
            <>
              <Conditional
                condition={!!data.items.length}
                onTrue={() => (
                  <Row>
                    {data.items.map(product => (
                      <Col key={product.id} xs={12} sm={6} md={6} lg={4} className="mb-4">
                        <ProductCard product={product} />
                      </Col>
                    ))}
                  </Row>
                )}
                onFalse={() => (
                  <div className="text-center">
                    <h4>No products found</h4>
                    <p>Try adjusting your search or filter settings.</p>
                  </div>
                )}
              />
              
              <Conditional
                condition={data.totalPages > 1}
                onTrue={() => (
                  <div className="mt-4">
                    <Pagination
                      currentPage={data.page}
                      totalPages={data.totalPages}
                      onPageChange={props.onPageChange}
                    />
                  </div>
                )}
              />
            </>
          )}
        />
      )}
    />
  );
};

