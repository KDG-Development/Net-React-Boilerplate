import { Col, Row } from "kdg-react";
import { Skeleton } from "../../../components/Skeleton";
import { ProductGridSkeleton } from "./ProductGridSkeleton";
import { FilterSidebarSkeleton } from "./FilterSidebarSkeleton";

export const CategoryPageSkeleton = () => {
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
            <Skeleton.Text width={100} />
          </div>
        </nav>

        {/* Category Title skeleton */}
        <Skeleton.Box width={250} height={40} className="mb-4" />

        {/* Subcategory Navigation skeleton */}
        <div className="mb-4 border-bottom border-bottom-light pb-4">
          <Skeleton.Text width={150} className="mb-3" />
          <div className="d-flex flex-wrap gap-2">
            <Skeleton.Button width={90} />
            <Skeleton.Button width={110} />
            <Skeleton.Button width={85} />
            <Skeleton.Button width={100} />
            <Skeleton.Button width={95} />
          </div>
        </div>

        {/* Mobile filter button skeleton */}
        <div className="d-lg-none mb-3">
          <Skeleton.Button width={100} />
        </div>

        <Row>
          {/* Filter Sidebar skeleton - Desktop only */}
          <Col md={3} className="d-none d-lg-block">
            <FilterSidebarSkeleton />
          </Col>

          {/* Products Grid skeleton */}
          <Col md={9}>
            <ProductGridSkeleton />
          </Col>
        </Row>
      </Col>
    </Row>
  );
};

