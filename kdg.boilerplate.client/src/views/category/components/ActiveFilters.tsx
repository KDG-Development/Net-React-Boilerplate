import { Badge, Clickable, Conditional, Enums, Icon } from "kdg-react";
import { ProductFilterParams, DEFAULT_PRODUCT_FILTERS } from "../../../types/product/product";

type ActiveFiltersProps = {
  filter: ProductFilterParams;
  onFilterChange: (filter: ProductFilterParams) => void;
};

const formatPriceRange = (minPrice: number | null, maxPrice: number | null): string => {
  if (minPrice !== null && maxPrice !== null) {
    return `$${minPrice} - $${maxPrice}`;
  }
  if (minPrice !== null) {
    return `$${minPrice} & Above`;
  }
  if (maxPrice !== null) {
    return `Under $${maxPrice}`;
  }
  return '';
};

export const ActiveFilters = (props: ActiveFiltersProps) => {
  const hasPriceFilter = props.filter.minPrice !== null || props.filter.maxPrice !== null;
  const hasSearchFilter = !!props.filter.search;
  const hasFavoritesFilter = props.filter.favoritesOnly;
  const hasAnyFilter = hasPriceFilter || hasSearchFilter || hasFavoritesFilter;

  return (
    <Conditional
      condition={hasAnyFilter}
      onTrue={() => (
        <div className="d-flex flex-wrap align-items-center gap-2 mb-3">
          <Conditional
            condition={hasSearchFilter}
            onTrue={() => (
              <Clickable onClick={() => props.onFilterChange({ ...props.filter, search: null })}>
                <Badge color={Enums.Color.Secondary} className="d-inline-flex align-items-center gap-1 py-2 px-3">
                  Search: {props.filter.search}
                  <Icon icon={(x) => x.cilX} size="sm" />
                </Badge>
              </Clickable>
            )}
          />
          <Conditional
            condition={hasPriceFilter}
            onTrue={() => (
              <Clickable onClick={() => props.onFilterChange({ ...props.filter, minPrice: null, maxPrice: null })}>
                <Badge color={Enums.Color.Secondary} className="d-inline-flex align-items-center gap-1 py-2 px-3">
                  {formatPriceRange(props.filter.minPrice, props.filter.maxPrice)}
                  <Icon icon={(x) => x.cilX} size="sm" />
                </Badge>
              </Clickable>
            )}
          />
          <Conditional
            condition={hasFavoritesFilter}
            onTrue={() => (
              <Clickable onClick={() => props.onFilterChange({ ...props.filter, favoritesOnly: false })}>
                <Badge color={Enums.Color.Secondary} className="d-inline-flex align-items-center gap-1 py-2 px-3">
                  Favorites Only
                  <Icon icon={(x) => x.cilX} size="sm" />
                </Badge>
              </Clickable>
            )}
          />
          <Clickable onClick={() => props.onFilterChange(DEFAULT_PRODUCT_FILTERS)}>
            <span className="text-muted small text-decoration-underline">Clear all</span>
          </Clickable>
        </div>
      )}
    />
  );
};

