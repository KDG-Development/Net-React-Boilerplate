import { Clickable, Radio } from "kdg-react";
import { PriceRange, PRICE_RANGES } from "../../../types/product/product";
import { ControlledSearchInput } from "../../_common/components/ControlledSearchInput";
import { useHeaderHeight } from "../../../hooks/useHeaderHeight";

type FilterSidebarProps = {
  selectedPriceRange: PriceRange;
  onPriceRangeChange: (range: PriceRange) => void;
  search: string | null;
  onSearchChange: (term: string | null) => void;
};

export const FilterSidebar = (props: FilterSidebarProps) => {
  const headerHeight = useHeaderHeight();

  return (
    <div className="filter-sidebar position-sticky pt-4" style={{ top: headerHeight, maxHeight: `calc(100vh - ${headerHeight}px)`, overflowY: 'auto' }}>
      <div className="mb-4">
        <ControlledSearchInput
          value={props.search}
          onSearch={props.onSearchChange}
        />
      </div>
      <div className="mb-4">
        <h6 className="fw-bold mb-3">Prices</h6>
        <div className="d-flex flex-column gap-2">
          {Object.values(PRICE_RANGES).map(range => (
            <div key={range.id} className="d-flex align-items-center gap-2">
            <Radio
              key={range.id}
              name="priceRange"
              value={props.selectedPriceRange.id === range.id}
              onChange={() => props.onPriceRangeChange(range)}
            />
            <Clickable onClick={() => props.onPriceRangeChange(range)}>
              <label>{range.label}</label>
            </Clickable>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
