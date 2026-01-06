import { Col, Row } from "kdg-react";
import { Header } from "../_common/templates/components/Header";
import { Footer } from "../_common/templates/components/Footer";
import { HeroBanner } from "../../components/HeroBanner/HeroBanner";

export const Home = () => (
  <>
    <Header />
    <HeroBanner />
    <Row>
      <Col md={12}>
        {/* Homepage content goes here */}
      </Col>
    </Row>
    <Footer />
  </>
);
