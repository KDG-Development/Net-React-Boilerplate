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

const ProductCard = (props: { product: TProduct }) => (
  <Card
    className="shadow-none"
    body={{
      content: (
        <>
          <div className="mb-3 d-flex align-items-center justify-content-center product-image-container">
            <Image 
              src={props.product.image || placeholderImage} 
              className="img-fluid product-image"
            />
          </div>
          <h6 className="card-title mb-2">{props.product.name}</h6>
          {props.product.description && (
            <p className="card-text text-muted small mb-2 text-truncate">
              {props.product.description}
            </p>
          )}
          <p className="card-text text-primary fw-bold mt-auto mb-0">
            {formatCurrency(props.product.price)}
          </p>
        </>
      ),
    }}
  />
);

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

