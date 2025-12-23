import { Card, Col, Image, Loader, Row } from "kdg-react";
import { TProduct } from "../../../types/product/product";
import { PaginatedResponse } from "../../../types/common/pagination";
import { formatCurrency } from "../../../util/format";
import { Pagination } from "./Pagination";

type ProductGridProps = {
  data: PaginatedResponse<TProduct> | null;
  loading: boolean;
  onPageChange: (page: number) => void;
};

const ProductCard = (props: { product: TProduct }) => (
  <Card
    body={{
      content: (
        <>
          <div className="bg-light mb-3 d-flex align-items-center justify-content-center product-image-container">
            {props.product.image ? (
              <Image 
                src={props.product.image} 
                className="img-fluid product-image"
              />
            ) : (
              <span className="text-muted">No Image</span>
            )}
          </div>
          <h6 className="card-title mb-2">{props.product.name}</h6>
          <p className="card-text text-primary fw-bold mt-auto mb-0">
            {formatCurrency(props.product.price)}
          </p>
        </>
      ),
    }}
  />
);

export const ProductGrid = (props: ProductGridProps) => {
  if (props.loading) {
    return <Loader />;
  }

  if (!props.data || props.data.items.length === 0) {
    return (
      <div className="text-center py-5 text-muted">
        <p className="mb-0">No products found in this category.</p>
      </div>
    );
  }

  return (
    <>
      <Row>
        {props.data.items.map(product => (
          <Col key={product.id} xs={12} sm={6} md={4} lg={3} className="mb-4">
            <ProductCard product={product} />
          </Col>
        ))}
      </Row>

      <div className="mt-4">
        <Pagination
          currentPage={props.data.page}
          totalPages={props.data.totalPages}
          onPageChange={props.onPageChange}
        />
      </div>
    </>
  );
};

