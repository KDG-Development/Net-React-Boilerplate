import { Clickable, Conditional } from "kdg-react";

type PaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};
// TODO: this should use the Pagination component from kdg-react, but this ones a bit better atm
export const Pagination = (props: PaginationProps) => {
  const getPageNumbers = (): (number | 'ellipsis')[] => {
    const pages: (number | 'ellipsis')[] = [];
    const current = props.currentPage;
    const total = props.totalPages;

    // Always show first page
    pages.push(1);

    // Add ellipsis if current page is far from start
    if (current > 3) {
      pages.push('ellipsis');
    }

    // Add pages around current
    for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
      if (!pages.includes(i)) {
        pages.push(i);
      }
    }

    // Add ellipsis if current page is far from end
    if (current < total - 2) {
      pages.push('ellipsis');
    }

    // Always show last page
    if (total > 1 && !pages.includes(total)) {
      pages.push(total);
    }

    return pages;
  };

  return (
    <nav aria-label="Pagination">
      <ul className="pagination justify-content-center mb-0">
        <li className={`page-item ${props.currentPage === 1 ? 'disabled' : ''}`}>
          <Clickable
            className="page-link"
            onClick={() => props.currentPage > 1 && props.onPageChange(props.currentPage - 1)}
          >
            Previous
          </Clickable>
        </li>

        {getPageNumbers().map((page, idx) => (
          <Conditional
            key={page === 'ellipsis' ? `ellipsis-${idx}` : page}
            condition={page === 'ellipsis'}
            onTrue={() => (
              <li className="page-item disabled">
                <span className="page-link">...</span>
              </li>
            )}
            onFalse={() => (
              <li className={`page-item ${props.currentPage === page ? 'active' : ''}`}>
                <Clickable
                  className="page-link"
                  onClick={() => props.onPageChange(page as number)}
                >
                  {page}
                </Clickable>
              </li>
            )}
          />
        ))}

        <li className={`page-item ${props.currentPage === props.totalPages ? 'disabled' : ''}`}>
          <Clickable
            className="page-link"
            onClick={() => props.currentPage < props.totalPages && props.onPageChange(props.currentPage + 1)}
          >
            Next
          </Clickable>
        </li>
      </ul>
    </nav>
  );
};

