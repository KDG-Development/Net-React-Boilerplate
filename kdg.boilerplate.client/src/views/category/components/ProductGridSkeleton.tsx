import { Card, Col, Row } from "kdg-react";
import { Skeleton } from "../../../components/Skeleton";

type ProductGridSkeletonProps = {
  count?: number;
};

const ProductCardSkeleton = () => (
  <Card
    className="shadow-none"
    body={{
      content: (
        <>
          <div className="mb-3 d-flex align-items-center justify-content-center product-image-container">
            <Skeleton.Box width="100%" height={150} />
          </div>
          <Skeleton.Text width="80%" className="mb-2" />
          <Skeleton.Text width="60%" className="mb-2" />
          <Skeleton.Text width="40%" className="mt-auto" />
        </>
      ),
    }}
  />
);

export const ProductGridSkeleton = (props: ProductGridSkeletonProps) => {
  const count = props.count || 6;

  return (
    <>
      <Row>
        {Array.from({ length: count }).map((_, idx) => (
          <Col key={idx} xs={12} sm={6} md={6} lg={4} className="mb-4">
            <ProductCardSkeleton />
          </Col>
        ))}
      </Row>

      {/* Pagination skeleton */}
      <div className="mt-4 d-flex justify-content-center gap-2">
        <Skeleton.Button width={80} />
        <Skeleton.Button width={40} />
        <Skeleton.Button width={40} />
        <Skeleton.Button width={40} />
        <Skeleton.Button width={80} />
      </div>
    </>
  );
};

