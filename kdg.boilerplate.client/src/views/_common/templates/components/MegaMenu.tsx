import { useState, useRef, useEffect, useCallback } from "react";
import { Clickable, Icon, Loader } from "kdg-react";

// Recursive category type with dictionary children for O(1) lookups
export type CategoryNode = {
  label: string;
  slug: string;
  children?: CategoryTree;
};

export type CategoryTree = Record<string, CategoryNode>;

type MegaMenuProps = {
  trigger: React.ReactNode;
  categories: CategoryTree | null;
  loading?: boolean;
  onSelect?: (slug: string) => void;
};

type MegaMenuPanelProps = {
  items: CategoryTree;
  selectedId: string | null;
  onHover: (id: string) => void;
  onClick: (id: string, item: CategoryNode) => void;
  isLast: boolean;
};

const MegaMenuPanel = (props: MegaMenuPanelProps) => {
  const [hoveredId, setHoveredId] = useState<string | null>(null);

  return (
    <div
      className={`overflow-auto ${props.isLast ? "" : "border-end"}`}
      style={{ minWidth: 220, maxHeight: 400 }}
    >
      {Object.entries(props.items).map(([id, item]) => (
        <Clickable
          key={id}
          onClick={() => props.onClick(id, item)}
          className={`d-block w-100 text-start ${
            props.selectedId === id ? "bg-secondary bg-opacity-25" : 
            hoveredId === id ? "bg-light" : ""
          }`}
        >
          <div
            className="d-flex align-items-center w-100 px-3 py-2"
            onMouseEnter={() => {
              setHoveredId(id);
              props.onHover(id);
            }}
            onMouseLeave={() => setHoveredId(null)}
          >
            <span className="flex-grow-1">{item.label}</span>
            {item.children && Object.keys(item.children).length > 0 && (
              <Icon icon={(x) => x.cilChevronRight} size="sm" className="flex-shrink-0 ms-2" />
            )}
          </div>
        </Clickable>
      ))}
    </div>
  );
};

export const MegaMenu = (props: MegaMenuProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedPath, setSelectedPath] = useState<string[]>([]);
  const containerRef = useRef<HTMLDivElement>(null);

  // Close menu when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
        setSelectedPath([]);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  // Close on Escape key
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
        setSelectedPath([]);
      }
    };

    if (isOpen) {
      document.addEventListener("keydown", handleEscape);
    }
    return () => {
      document.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen]);

  const handleToggle = () => {
    setIsOpen(!isOpen);
    if (isOpen) {
      setSelectedPath([]);
    }
  };

  const handleHover = useCallback(
    (depth: number, id: string) => {
      setSelectedPath([...selectedPath.slice(0, depth), id]);
    },
    [selectedPath]
  );

  const handleClick = useCallback(
    (_depth: number, _id: string, item: CategoryNode) => {
      props.onSelect?.(item.slug);
      setIsOpen(false);
      setSelectedPath([]);
    },
    [props.onSelect]
  );

  // Build panels by traversing the selected path
  const panels = props.categories
    ? selectedPath.reduce(
        (acc, id, i) => {
          const children = acc.current[id]?.children;
          if (children && Object.keys(children).length > 0) {
            acc.panels.push({ items: children, selectedId: selectedPath[i + 1] ?? null });
            acc.current = children;
          }
          return acc;
        },
        {
          panels: [{ items: props.categories, selectedId: selectedPath[0] ?? null }] as { items: CategoryTree; selectedId: string | null }[],
          current: props.categories,
        }
      ).panels
    : [];

  return (
    <div className="position-relative d-inline-block" ref={containerRef}>
      <Clickable onClick={handleToggle} className="py-2">
        <div className="d-flex align-items-center">
          {props.trigger}
          <Icon icon={(x) => x.cilChevronBottom} size="sm" className="ms-1" />
        </div>
      </Clickable>

      {isOpen && (
        <div
          className="position-absolute start-0 bg-white border rounded shadow"
          style={{ top: "100%", zIndex: 1000, minWidth: 200 }}
        >
          {props.loading || !props.categories ? (
            <div className="p-4 d-flex justify-content-center">
              <Loader />
            </div>
          ) : (
            <div className="d-flex">
              {panels.map((panel, depth) => (
                <MegaMenuPanel
                  key={depth}
                  items={panel.items}
                  selectedId={panel.selectedId}
                  onHover={(id) => handleHover(depth, id)}
                  onClick={(id, item) => handleClick(depth, id, item)}
                  isLast={depth === panels.length - 1}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
