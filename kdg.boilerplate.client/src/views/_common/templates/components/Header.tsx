import { Clickable, Icon, Image, Nav, Row, TextInput, useAppNavigation } from "kdg-react";
import { Link } from "react-router-dom";
import logo from "../../../../assets/images/logo.png";
import { CategoryMegaMenu } from "./CategoryMegaMenu";
import { CartWidget } from "./CartWidget";
import { ROUTE_PATH } from "../../../../routing/AppRouter";
import { useAuthContext } from "../../../../context/AuthContext";

export const Header = () => {
  const user = useAuthContext();
  const navigate = useAppNavigation();
  return (
    <>
      {/* top header */}
      <div className="bg-light small">
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
      <div className="py-4 border-bottom border-bottom-light">
        <Row>
          <div className="d-flex align-items-center">
            <Link to={ROUTE_PATH.Home}>
              <Image src={logo} width={150} />
            </Link>
            <Nav
              navClassName="flex-grow-1 gap-3 mx-3"
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
            <div className="w-25">
              <TextInput
                placeholder="search"
                
                value={null}
                onChange={() => {}}
                icon={{
                  content: <Icon icon={(x) => x.cilSearch} />,
                }}
              />
            </div>
            <CartWidget />
          </div>
        </Row>
      </div>
    </>
  );
};
