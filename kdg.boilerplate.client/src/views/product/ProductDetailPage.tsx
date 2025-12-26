import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Col, Row, Conditional, EntityConditional, Image, Icon, Clickable, NumberInput, ActionButton } from "kdg-react";
import { BaseTemplate } from "../_common/templates/BaseTemplate";
import { getProductById } from "../../api/products";
import { TProductDetail } from "../../types/product/product";
import { useCartContext } from "../../context/CartContext";
import { formatCurrency } from "../../util/format";
import { ROUTE_PATH } from "../../routing/AppRouter";
import { ProductDetailSkeleton } from "./components/ProductDetailSkeleton";
import placeholderImage from "../../assets/images/logo.png";

export const ProductDetailPage = () => {
  const { productId } = useParams<{ productId: string }>();
  const { addItem } = useCartContext();

  const [product, setProduct] = useState<TProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  useEffect(() => {
    if (!productId) {
      setError("Product ID is required");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    getProductById({
      productId,
      success: (data) => {
        setProduct(data);
        setSelectedImageIndex(0);
        setLoading(false);
      },
      errorHandler: (e) => {
        if (e.status === 404) {
          setError("Product not found");
        } else {
          setError("Failed to load product");
        }
        setLoading(false);
      },
    });
  }, [productId]);

  const handleAddToCart = () => {
    if (!product) return;
    addItem(
      {
        id: product.id,
        name: product.name,
        description: product.description,
        price: product.price,
        images: product.images,
      },
      quantity
    );
    setQuantity(1);
  };

  const handleQuantityChange = (value: number | null) => {
    if (value !== null && value >= 1) {
      setQuantity(value);
    }
  };

  const selectedImage = product?.images[selectedImageIndex]?.src ?? product?.images[0]?.src ?? placeholderImage;

  return (
    <BaseTemplate>
      <Conditional
        condition={loading}
        onTrue={() => <ProductDetailSkeleton />}
        onFalse={() => (
          <Conditional
            condition={!!(error || !product)}
            onTrue={() => (
              <Row>
                <Col md={12}>
                  <div className="text-center py-5">
                    <h4 className="text-muted">{error || "Product not found"}</h4>
                    <Link to={ROUTE_PATH.Products} className="btn btn-primary mt-3">
                      Browse Products
                    </Link>
                  </div>
                </Col>
              </Row>
            )}
            onFalse={() => (
              <EntityConditional
                entity={product}
                render={(prod) => (
                  <Row>
                    <Col md={12}>
                      {/* Breadcrumbs */}
                      <nav aria-label="breadcrumb" className="mb-4">
                        <ol className="breadcrumb">
                          <li className="breadcrumb-item">
                            <Link to={ROUTE_PATH.Home}>Home</Link>
                          </li>
                          <li className="breadcrumb-item">
                            <Link to={ROUTE_PATH.Products}>Products</Link>
                          </li>
                          {prod.breadcrumbs.map((crumb) => (
                            <li key={crumb.id} className="breadcrumb-item">
                              <Link to={`${ROUTE_PATH.Products}?category=${crumb.slug}`}>
                                {crumb.name}
                              </Link>
                            </li>
                          ))}
                          <li className="breadcrumb-item active" aria-current="page">
                            {prod.name}
                          </li>
                        </ol>
                      </nav>

                      <Row>
                        {/* Product Image */}
                        <Col md={5} className="mb-4 mb-md-0">
                          <div className="product-detail-image-container">
                            <Image
                              src={selectedImage}
                              className="img-fluid product-detail-image"
                            />
                          </div>

                          {/* Thumbnail Gallery */}
                          <Conditional
                            condition={!!prod.images.length}
                            onTrue={() => (
                              <div className="product-thumbnail-gallery mt-3">
                                {prod.images.map((img, index) => (
                                  <Clickable
                                    key={img.id}
                                    onClick={() => setSelectedImageIndex(index)}
                                    className={`product-thumbnail ${index === selectedImageIndex ? 'active' : ''}`}
                                  >
                                    <Image
                                      src={img.src}
                                      className="img-fluid"
                                    />
                                  </Clickable>
                                ))}
                              </div>
                            )}
                            onFalse={() => null}
                          />
                        </Col>

                        {/* Product Info */}
                        <Col md={7}>
                          <h1 className="h3 mb-3">{prod.name}</h1>

                          <Conditional
                            condition={!!prod.description}
                            onTrue={() => (
                              <p className="text-muted mb-4">{prod.description}</p>
                            )}
                            onFalse={() => null}
                          />

                          <p className="h4 text-primary fw-bold mb-4">
                            {formatCurrency(prod.price)}
                          </p>

                          {/* Quantity and Add to Cart */}
                          <div className="d-flex align-items-center gap-3">
                            <div>
                              <NumberInput
                                onChange={handleQuantityChange}
                                value={quantity}
                                min={1}
                                allowDecimals={false}
                                hideDefaultHelperText
                              />
                            </div>
                            <ActionButton
                              onClick={handleAddToCart}
                            >
                              <Icon className="me-2" icon={(x) => x.cilCart} />
                              Add to Cart
                            </ActionButton>
                          </div>
                        </Col>
                      </Row>
                    </Col>
                  </Row>
                )}
              />
            )}
          />
        )}
      />
    </BaseTemplate>
  );
};

