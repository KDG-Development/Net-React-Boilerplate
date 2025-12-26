import { useState } from "react";
import { Clickable, Icon, Image, Nav, Row, useAppNavigation } from "kdg-react";
import { Link, useLocation } from "react-router-dom";
import logo from "../../../../assets/images/logo.png";
import { CategoryMegaMenu } from "./CategoryMegaMenu";
import { CartWidget } from "./CartWidget";
import { ROUTE_PATH } from "../../../../routing/AppRouter";
import { useAuthContext } from "../../../../context/AuthContext";
import { getResetSearchParams } from "../../../../hooks/useProductFilters";
import { ControlledSearchInput } from "../../components/ControlledSearchInput";
import { Drawer } from "../../../../components/Drawer";

export const Header = () => {
  const user = useAuthContext();
  const navigate = useAppNavigation();
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const handleHeaderSearch = (value: string | null) => {
    const searchParams = getResetSearchParams(value);
    const queryString = searchParams.toString();
    navigate(`${ROUTE_PATH.Products}${queryString ? `?${queryString}` : ''}`);
  };

  // Only show current search value when on products page
  const isOnProductsPage = location.pathname === ROUTE_PATH.Products;
  const currentSearch = isOnProductsPage 
    ? new URLSearchParams(location.search).get('search') 
    : null;

  return (
    <>
      <div className="position-sticky top-0 bg-white" style={{ zIndex: 1030 }}>
        {/* top header - hidden on mobile */}
        <div className="bg-light small d-none d-lg-block">
        <div className="px-5 py-1">
          <div className="d-flex align-items-center justify-content-between small">
            <div><a href='tel:111-111-1111'>111-111-1111</a> | <a href='mailto:contact@email.com'>contact@email.com</a></div>
            <Clickable onClick={() => {
              if (user.user) {
                user.logout();
              } else {
                navigate(ROUTE_PATH.LOGIN);
              }
            }}>
              <div>
                Hello, {user.user ? user.user.user.email : 'Guest'}!
                <span className="text-info ms-1">
                  ({user.user ? 'Logout?' : 'Login?'})
                </span>
              </div>
            </Clickable>
          </div>
        </div>
      </div>
      {/* Primary header */}
      <div className="py-2 py-lg-4 border-bottom border-bottom-light">
        <Row>
          <div className="d-flex align-items-center">
            {/* Hamburger toggle - visible only on mobile, on the left */}
            <Icon onClick={() => setMobileMenuOpen(true)} icon={(x) => x.cilMenu} className="d-lg-none" />
            {/* Logo - centered on mobile */}
            <Link to={ROUTE_PATH.Home} className="d-lg-none mx-auto">
              <Image src={logo} width={150} />
            </Link>
            {/* Logo - left aligned on desktop */}
            <Link to={ROUTE_PATH.Home} className="d-none d-lg-block">
              <Image src={logo} width={150} />
            </Link>
            {/* Desktop navigation - hidden on mobile */}
            <Nav
              navClassName="flex-grow-1 gap-3 mx-3 d-none d-lg-flex"
              items={[
                {
                  key: "Products",
                  label: <CategoryMegaMenu />,
                  onClick: () => {
                    // nothing, handled by mega menu
                  },
                },
                {
                  key: "favorites",
                  label: "Favorites",
                  onClick: () => {
                    // Navigate to favorites page
                    console.log("Navigate to favorites");
                  },
                },
                {
                  key: "my-account",
                  label: "My Account",
                  onClick: () => {
                    // Navigate to account page
                    console.log("Navigate to my account");
                  },
                },
              ]}
            />
            {/* Desktop search - hidden on mobile */}
            <div className="d-none d-lg-block w-auto">
              <ControlledSearchInput
                placeholder="Search products..."
                value={currentSearch}
                onSearch={handleHeaderSearch}
              />
            </div>
            <CartWidget />
          </div>
        </Row>
        {/* Mobile search row - visible only on mobile */}
        <Row className="d-lg-none mt-3">
          <ControlledSearchInput
            placeholder="Search products..."
            value={currentSearch}
            onSearch={handleHeaderSearch}
          />
        </Row>
      </div>
      </div>

      {/* Mobile navigation drawer - CSS hides on desktop to handle resize edge case */}
      <div className="d-lg-none">
        <Drawer
          isOpen={mobileMenuOpen}
          onClose={() => setMobileMenuOpen(false)}
          header={() => <h5 className="mb-0">Menu</h5>}
          position="start"
          width="75%"
          footer={() => (
            <div className="small">
              <div className="mb-2">
                <a href="tel:111-111-1111">111-111-1111</a>
                <span className="mx-1">|</span>
                <a href="mailto:contact@email.com">contact@email.com</a>
              </div>
              <Clickable onClick={() => {
                if (user.user) {
                  user.logout();
                } else {
                  navigate(ROUTE_PATH.LOGIN);
                }
                setMobileMenuOpen(false);
              }}>
                <div>
                  Hello, {user.user ? user.user.user.email : 'Guest'}!
                  <span className="text-info ms-1">
                    ({user.user ? 'Logout?' : 'Login?'})
                  </span>
                </div>
              </Clickable>
            </div>
          )}
        >
          <nav className="d-flex flex-column gap-3">
            <CategoryMegaMenu 
              onCategorySelect={() => setMobileMenuOpen(false)} 
              variant="mobile"
            />
            <Clickable
              onClick={() => {
                console.log("Navigate to favorites");
                setMobileMenuOpen(false);
              }}
            >
              Favorites
            </Clickable>
            <Clickable
              onClick={() => {
                console.log("Navigate to my account");
                setMobileMenuOpen(false);
              }}
            >
              My Account
            </Clickable>
          </nav>
        </Drawer>
      </div>
    </>
  );
};
