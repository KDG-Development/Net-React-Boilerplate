import { Col, Image, Row } from "kdg-react";
import logo from "../../../../assets/images/logo.png";

export const Footer = () => (
  <>
    <div className=" bg-light py-4 border-top border-top-light">
      <Row>
        <Col md={6} className="">
          <Image src={logo} width={150} />
        </Col>
        <Col md={2}>nav 1</Col>
        <Col md={2}>nav 2</Col>
        <Col md={2}>nav 3</Col>
      </Row>
    </div>
    <div className="small text-muted text-center">
      <small>{`${new Date().getFullYear()} © Copyright Blackhawk, Inc. - All Rights Reserved.`}</small>
    </div>
  </>
)