import { Skeleton } from "../../../components/Skeleton";

export const FilterSidebarSkeleton = () => {
  return (
    <div className="filter-sidebar">
      <div className="mb-4">
        <Skeleton.Text width={60} className="mb-3" />
        <div className="d-flex flex-column gap-2">
          {Array.from({ length: 6 }).map((_, idx) => (
            <div key={idx} className="d-flex align-items-center gap-2">
              <Skeleton.Box width={16} height={16} />
              <Skeleton.Text width={80 + (idx % 3) * 20} />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

