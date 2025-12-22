import { Col, Row } from "kdg-react"
import { Header } from "./components/Header"
import { Footer } from "./components/Footer"

type TBaseTemplateProps = {
  children:React.ReactNode
}

export const BaseTemplate = (props:TBaseTemplateProps) => {
  return (
    <>
      <Header />
      <Row>
        <Col md={12}>{props.children}</Col>
      </Row>
      <Footer />
    </>
  )
}