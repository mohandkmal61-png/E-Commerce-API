import { useState } from "react";
import type { ProductDto } from "../../types/api";
import {
  getProductFallbackImage,
  getRemoteProductImageUrl,
} from "../../data/fashionImages";
import { Icon } from "../ui/Icon";
import { useBasket } from "../../contexts/BasketContext";

interface ProductCardProps {
  product: ProductDto;
  index: number;
}

export function ProductCard({ product, index }: ProductCardProps) {
  const { addProduct, loading: basketLoading, saving } = useBasket();
  const [feedback, setFeedback] = useState<string | null>(null);
  const [addError, setAddError] = useState<string | null>(null);
  const imageUrl = getRemoteProductImageUrl(product.PictureUrl);
  const fallbackImageUrl = getProductFallbackImage(product);
  const [failedImageUrl, setFailedImageUrl] = useState<string | null>(null);
  const imageSource =
    imageUrl && failedImageUrl !== imageUrl
      ? imageUrl
      : fallbackImageUrl && failedImageUrl !== fallbackImageUrl
        ? fallbackImageUrl
        : null;
  const isFallbackImage = imageSource !== null && imageSource !== imageUrl;

  async function handleAddToBag() {
    setFeedback(null);
    setAddError(null);
    if (await addProduct(product)) {
      setFeedback("Added to bag");
    } else {
      setAddError("Could not add this item. Please retry from your bag.");
    }
  }

  return (
    <article className={`product-card product-card--${index + 1}`}>
      <a
        aria-label={`Explore ${product.name}`}
        className="product-image-link"
        href={`/products/${product.id}`}
      >
        {imageSource ? (
          <img
            alt={isFallbackImage ? `Style reference for ${product.name}` : product.name}
            loading="lazy"
            onError={() => setFailedImageUrl(imageSource)}
            src={imageSource}
          />
        ) : (
          <span className="product-image-placeholder">
            <Icon name="sparkle" />
            <span>Image unavailable</span>
          </span>
        )}
        {isFallbackImage && <span className="product-image-note">Style reference</span>}
        <span aria-hidden="true" className="product-quick-link">
          <Icon name="arrow-right" />
        </span>
      </a>
      <div className="product-meta">
        <div>
          <span className="product-category">{product.productType}</span>
          <h3>
            <a href={`/products/${product.id}`}>{product.name}</a>
          </h3>
        </div>
        <span className="product-price">{product.price.toFixed(2)}</span>
      </div>
      <p className="product-description">{product.description}</p>
      <button
        className="product-add-button"
        disabled={basketLoading || saving}
        onClick={() => void handleAddToBag()}
        type="button"
      >
        {saving ? "Adding..." : "Add to bag"}
        <Icon name="arrow-right" />
      </button>
      {feedback && <span className="product-add-feedback" role="status">{feedback}</span>}
      {addError && <span className="product-add-error" role="alert">{addError}</span>}
    </article>
  );
}
