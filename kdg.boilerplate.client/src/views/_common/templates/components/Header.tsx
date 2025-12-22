import { Icon, Image, Row, TextInput } from "kdg-react";
import logo from "../../../../assets/images/logo.png";

export const Header = () => {
  return (
    <>
      {/* top header */}
      <div className="bg-light">
        <div className="px-5 py-1">
          <div className="d-flex align-items-center justify-content-between small">
            <span>111-111-1111 | contact@email.com</span>
            <span>Hello, Guest! [login]</span>
          </div>
        </div>
      </div>
      {/* Primary header */}
      <div className="py-4 border-bottom border-bottom-light">
        <Row>
          <div className="d-flex align-items-center justify-content-between">
            <div className="flex-grow-1">
              <div className="d-flex align-items-center">
                <Image src={logo} width={150} />
                <div className="mx-3">
                  todo: nav, mega menu
                </div>
              </div>
            </div>
            <TextInput
              placeholder="Search"
              className="w-25"
              value={null}
              onChange={() => {}}
            />
            <Icon
              icon={x => x.cilCart}
              className="ms-3"
              size="xl"
              onClick={() => {}}
            />
          </div>
        </Row>
      </div>
    </>
  )
}