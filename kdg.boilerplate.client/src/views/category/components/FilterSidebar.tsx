import { useState } from "react";
import { ActionButton, Clickable, Conditional, Radio, TextInput } from "kdg-react";
import { ProductFilterParams, DEFAULT_PRICE_RANGE_OPTIONS } from "../../../types/product/product";
import { useHeaderHeight } from "../../../hooks/useHeaderHeight";
import { useUserContext } from "../../../context/UserContext";
import { onEnterKey } from "../../../util/keyboard";

type FilterSidebarProps = {
  filter: ProductFilterParams;
  onFilterChange: (filter: ProductFilterParams) => void;
};

const filtersEqual = (a: ProductFilterParams, b: ProductFilterParams): boolean => {
  return a.minPrice === b.minPrice
    && a.maxPrice === b.maxPrice
    && a.search === b.search
    && a.favoritesOnly === b.favoritesOnly;
};

const isPriceRangeSelected = (filter: ProductFilterParams, rangeMin: number | null, rangeMax: number | null): boolean => {
  return filter.minPrice === rangeMin && filter.maxPrice === rangeMax;
};

export const FilterSidebar = (props: FilterSidebarProps) => {
  const headerHeight = useHeaderHeight();
  const { user } = useUserContext();
  const showFavoritesFilter = !!user.organization;

  const [draft, setDraft] = useState<ProductFilterParams>(props.filter);
  const [prevFilter, setPrevFilter] = useState<ProductFilterParams>(props.filter);

  // Reset draft when props.filter changes (render-time state adjustment)
  if (!filtersEqual(props.filter, prevFilter)) {
    setDraft(props.filter);
    setPrevFilter(props.filter);
  }

  const hasChanges = !filtersEqual(draft, props.filter);

  const handleApply = () => {
    props.onFilterChange(draft);
  };

  return (
    <div className="filter-sidebar position-sticky pt-4" style={{ top: headerHeight, maxHeight: `calc(100vh - ${headerHeight}px)`, overflowY: 'auto' }} onKeyDown={onEnterKey(handleApply)}>
      <Conditional
        condition={showFavoritesFilter}
        onTrue={() => (
          <div className="mb-4">
            <h6 className="fw-bold mb-3">Favorites</h6>
            <div className="d-flex align-items-center gap-2">
              <Radio
                name="favoritesOnly"
                value={draft.favoritesOnly}
                onChange={() => setDraft({ ...draft, favoritesOnly: !draft.favoritesOnly })}
              />
              <Clickable onClick={() => setDraft({ ...draft, favoritesOnly: !draft.favoritesOnly })}>
                <label>Show favorites only</label>
              </Clickable>
            </div>
          </div>
        )}
      />
      <div className="mb-4">
        <TextInput
          placeholder="Search..."
          value={draft.search}
          onChange={(value) => setDraft({ ...draft, search: value })}
          type="search"
        />
      </div>
      <div className="mb-4">
        <h6 className="fw-bold mb-3">Prices</h6>
        <div className="d-flex flex-column gap-2">
          {DEFAULT_PRICE_RANGE_OPTIONS.map(option => {
            const key = `${option.minPrice ?? 'null'}-${option.maxPrice ?? 'null'}`;
            return (
              <div key={key} className="d-flex align-items-center gap-2">
                <Radio
                  name="priceRange"
                  value={isPriceRangeSelected(draft, option.minPrice, option.maxPrice)}
                  onChange={() => setDraft({ ...draft, minPrice: option.minPrice, maxPrice: option.maxPrice })}
                />
                <Clickable onClick={() => setDraft({ ...draft, minPrice: option.minPrice, maxPrice: option.maxPrice })}>
                  <label>{option.label}</label>
                </Clickable>
              </div>
            );
          })}
        </div>
      </div>
      <div className="mt-3">
        <ActionButton onClick={handleApply} disabled={!hasChanges} className="w-100">
          Apply Filters
        </ActionButton>
      </div>
    </div>
  );
};
