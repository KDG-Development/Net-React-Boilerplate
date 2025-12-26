import { Clickable, Radio } from "kdg-react";
import { PriceRange, PRICE_RANGES } from "../../../types/product/product";

type FilterSidebarProps = {
  selectedPriceRange: PriceRange;
  onPriceRangeChange: (range: PriceRange) => void;
};

export const FilterSidebar = (props: FilterSidebarProps) => {
  return (
    <div className="filter-sidebar">
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

