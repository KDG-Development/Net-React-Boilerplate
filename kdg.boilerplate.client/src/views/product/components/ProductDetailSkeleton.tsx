import { Col, Row } from "kdg-react";
import { Skeleton } from "../../../components/Skeleton";

export const ProductDetailSkeleton = () => {
  return (
    <Row>
      <Col md={12}>
        {/* Breadcrumbs skeleton */}
        <nav aria-label="breadcrumb" className="mb-4">
          <div className="d-flex gap-2 align-items-center">
            <Skeleton.Text width={50} />
            <span className="text-muted">/</span>
            <Skeleton.Text width={80} />
            <span className="text-muted">/</span>
            <Skeleton.Text width={120} />
          </div>
        </nav>

        <Row>
          {/* Product Image skeleton */}
          <Col md={5} className="mb-4 mb-md-0">
            <Skeleton.Box width="100%" height={400} />
            
            {/* Thumbnail Gallery skeleton */}
            <div className="d-flex gap-2 mt-3">
              <Skeleton.Box width={70} height={70} />
              <Skeleton.Box width={70} height={70} />
              <Skeleton.Box width={70} height={70} />
              <Skeleton.Box width={70} height={70} />
            </div>
          </Col>

          {/* Product Info skeleton */}
          <Col md={7}>
            {/* Product Name */}
            <Skeleton.Box width="80%" height={32} className="mb-3" />
            
            {/* Description */}
            <Skeleton.Text width="100%" lines={3} className="mb-4" />
            
            {/* Price */}
            <Skeleton.Box width={120} height={36} className="mb-4" />
            
            {/* Quantity and Add to Cart */}
            <div className="d-flex align-items-center gap-3">
              <Skeleton.Box width={80} height={38} />
              <Skeleton.Button width={140} />
            </div>
          </Col>
        </Row>
      </Col>
    </Row>
  );
};

