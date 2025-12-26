import { Card, Col, Conditional, EntityConditional, Image, Row } from "kdg-react";
import { TProduct } from "../../../types/product/product";
import { PaginatedResponse } from "../../../types/common/pagination";
import { formatCurrency } from "../../../util/format";
import { Pagination } from "./Pagination";
import { ProductGridSkeleton } from "./ProductGridSkeleton";
import placeholderImage from "../../../assets/images/logo.png";

type ProductGridProps = {
  data: PaginatedResponse<TProduct> | null;
  loading: boolean;
  onPageChange: (page: number) => void;
};

const ProductCard = (props: { product: TProduct }) => {
  const primaryImage = props.product.images[0]?.src ?? placeholderImage;

  return (
    <Card
      className="shadow-none"
      body={{
        content: (
          <>
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
        </>
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
              <Row>
                {data.items.map(product => (
                  <Col key={product.id} xs={12} sm={6} md={6} lg={4} className="mb-4">
                    <ProductCard product={product} />
                  </Col>
                ))}
              </Row>
    
              <div className="mt-4">
                <Pagination
                  currentPage={data.page}
                  totalPages={data.totalPages}
                  onPageChange={props.onPageChange}
                />
              </div>
            </>
          )}
        />
      )}
    />
  );
};

