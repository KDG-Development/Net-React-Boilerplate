import { useState, useRef, useEffect, useCallback } from "react";
import { Clickable, Icon, Loader, Conditional, EntityConditional } from "kdg-react";

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
  variant?: "desktop" | "mobile";
};

// Mobile: recursive accordion item
type MobileItemProps = {
  item: CategoryNode;
  onSelect: (slug: string) => void;
  depth?: number;
};

const MobileItem = (props: MobileItemProps) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const hasChildren = props.item.children && Object.keys(props.item.children).length > 0;
  const depth = props.depth ?? 0;

  return (
    <div>
      <div className="d-flex align-items-center" style={{ paddingLeft: depth * 16 }}>
        <Clickable onClick={() => props.onSelect(props.item.slug)} className="d-inline-block py-2">
          {props.item.label}
        </Clickable>
        <Conditional
          condition={!!hasChildren}
          onTrue={() => (
            <Clickable onClick={() => setIsExpanded(!isExpanded)} className="ms-auto">
              <Icon
                className='border'
                icon={(x) => (isExpanded ? x.cilMinus : x.cilPlus)} size="sm"
              />
            </Clickable>
          )}
          onFalse={() => null}
        />
      </div>
      <Conditional
        condition={isExpanded && !!hasChildren}
        onTrue={() => (
          <div className="ms-3 ps-3 border-start">
            {Object.entries(props.item.children!).map(([id, child]) => (
              <MobileItem key={id} item={child} onSelect={props.onSelect} depth={depth + 1} />
            ))}
          </div>
        )}
        onFalse={() => null}
      />
    </div>
  );
};

// Desktop: horizontal panel
type PanelProps = {
  items: CategoryTree;
  selectedId: string | null;
  onHover: (id: string) => void;
  onClick: (item: CategoryNode) => void;
  isLast: boolean;
};

const Panel = (props: PanelProps) => {
  const [hoveredId, setHoveredId] = useState<string | null>(null);

  return (
    <div
      className={`overflow-auto ${props.isLast ? "" : "border-end"}`}
      style={{ minWidth: 220, maxHeight: 400 }}
    >
      {Object.entries(props.items).map(([id, item]) => (
        <Clickable
          key={id}
          onClick={() => props.onClick(item)}
          className={`d-block w-100 text-start ${
            props.selectedId === id ? "bg-secondary bg-opacity-25" : hoveredId === id ? "bg-light" : ""
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
  const isMobile = props.variant === "mobile";

  // Close on outside click or escape (desktop only)
  useEffect(() => {
    if (isMobile || !isOpen) return;

    const handleClose = (e: MouseEvent | KeyboardEvent) => {
      if (e instanceof KeyboardEvent && e.key !== "Escape") return;
      if (e instanceof MouseEvent && containerRef.current?.contains(e.target as Node)) return;
      setIsOpen(false);
      setSelectedPath([]);
    };

    document.addEventListener("mousedown", handleClose);
    document.addEventListener("keydown", handleClose);
    return () => {
      document.removeEventListener("mousedown", handleClose);
      document.removeEventListener("keydown", handleClose);
    };
  }, [isOpen, isMobile]);

  const handleSelect = useCallback(
    (slug: string) => {
      props.onSelect?.(slug);
      setIsOpen(false);
      setSelectedPath([]);
    },
    [props.onSelect]
  );

  const handleHover = useCallback(
    (depth: number, id: string) => {
      setSelectedPath((prev) => [...prev.slice(0, depth), id]);
    },
    []
  );

  // Build panels for desktop by traversing selectedPath
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
          panels: [{ items: props.categories, selectedId: selectedPath[0] ?? null }] as {
            items: CategoryTree;
            selectedId: string | null;
          }[],
          current: props.categories,
        }
      ).panels
    : [];

  const content = (
    <Conditional
      condition={!!(props.loading || !props.categories)}
      onTrue={() => (
        <div className={isMobile ? "py-3" : "p-4 d-flex justify-content-center"}>
          <Loader />
        </div>
      )}
      onFalse={() => (
        <EntityConditional
          entity={props.categories}
          render={categories => (
            <Conditional
              condition={isMobile}
              onTrue={() => (
                <>
                  {Object.entries(categories).map(([id, item]) => (
                    <MobileItem key={id} item={item} onSelect={handleSelect} />
                  ))}
                </>
              )}
              onFalse={() => (
                <div className="d-flex">
                  {panels.map((panel, depth) => (
                    <Panel
                      key={depth}
                      items={panel.items}
                      selectedId={panel.selectedId}
                      onHover={(id) => handleHover(depth, id)}
                      onClick={(item) => handleSelect(item.slug)}
                      isLast={depth === panels.length - 1}
                    />
                  ))}
                </div>
              )}
            />
          )}
        />
      )}
    />
  );

  return (
    <div ref={containerRef} className={isMobile ? "" : "position-relative d-inline-block"}>
      <Clickable
        onClick={() => {
          setIsOpen(!isOpen);
          if (isOpen) setSelectedPath([]);
        }}
        className="py-2"
      >
        <div className="d-flex align-items-center">
          {props.trigger}
          <Icon
            icon={(x) => (isMobile ? (isOpen ? x.cilChevronBottom : x.cilChevronRight) : x.cilChevronBottom)}
            size="sm"
            className="ms-1"
          />
        </div>
      </Clickable>

      <Conditional
        condition={isOpen}
        onTrue={() => (
          <div
            className={
              isMobile
                ? "ps-3 border-start ms-2"
                : "position-absolute start-0 bg-white border rounded shadow"
            }
            style={isMobile ? undefined : { top: "100%", zIndex: 1000, minWidth: 200 }}
          >
            {content}
          </div>
        )}
        onFalse={() => null}
      />
    </div>
  );
};
