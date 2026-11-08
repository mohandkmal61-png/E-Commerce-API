import { useEffect, useState } from "react";
import { CollectionState } from "../components/ui/CollectionState";
import { Icon } from "../components/ui/Icon";
import { useBasket } from "../contexts/BasketContext";
import {
  getProductFallbackImage,
  getRemoteProductImageUrl,
} from "../data/fashionImages";
import { apiClient } from "../services/apiClient";
import type { ProductDto } from "../types/api";

interface ProductDetailsPageProps {
  productId: number;
}

export function ProductDetailsPage({ productId }: ProductDetailsPageProps) {
  const [product, setProduct] = useState<ProductDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [requestId, setRequestId] = useState(0);
  const [added, setAdded] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);
  const {
    addProduct,
    error: basketError,
    loading: basketLoading,
    retry: retryBasket,
    saving,
  } = useBasket();
  const imageUrl = product ? getRemoteProductImageUrl(product.PictureUrl) : null;
  const fallbackImageUrl = product ? getProductFallbackImage(product) : null;
  const [failedImageUrl, setFailedImageUrl] = useState<string | null>(null);
  const displayedImage =
    imageUrl && failedImageUrl !== imageUrl
      ? imageUrl
      : fallbackImageUrl && failedImageUrl !== fallbackImageUrl
        ? fallbackImageUrl
        : null;

  useEffect(() => {
    let isCurrentRequest = true;

    setLoading(true);
    setError(null);
    setProduct(null);
    setFailedImageUrl(null);

    apiClient
      .getProduct(productId)
      .then((result) => {
        if (isCurrentRequest) {
          setProduct(result);
        }
      })
      .catch((requestError: unknown) => {
        if (isCurrentRequest) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : "This product could not be loaded.",
          );
        }
      })
      .finally(() => {
        if (isCurrentRequest) {
          setLoading(false);
        }
      });

    return () => {
      isCurrentRequest = false;
    };
  }, [productId, requestId]);

  async function handleAddToBag() {
    if (!product) {
      return;
    }

    setAdded(false);
    setAddError(null);
    const saved = await addProduct(product);
    if (saved) {
      setAdded(true);
    } else {
      setAddError(basketError ?? "The item could not be added to your bag.");
    }
  }

  return (
    <section className="detail-page page-shell" aria-labelledby="product-details-heading">
      <a className="back-link" href="/#shop">
        <Icon name="arrow-left" /> Back to the collection
      </a>

      {loading ? (
        <CollectionState kind="loading" message="Loading product details..." />
      ) : error ? (
        <CollectionState
          kind="error"
          message={error}
          onRetry={() => setRequestId((attempt) => attempt + 1)}
        />
      ) : product ? (
        <div className="detail-layout">
          <div className="detail-image">
            {displayedImage ? (
              <img
                alt={displayedImage !== imageUrl ? `Style reference for ${product.name}` : product.name}
                onError={() => setFailedImageUrl(displayedImage)}
                src={displayedImage}
              />
            ) : (
              <span className="product-image-placeholder">
                <Icon name="sparkle" />
                <span>Image unavailable</span>
              </span>
            )}
            {displayedImage !== imageUrl && displayedImage && (
              <span className="product-image-note">Style reference</span>
            )}
          </div>
          <div className="detail-copy">
            <span className="eyebrow">{product.productType}</span>
            <h1 id="product-details-heading">{product.name}</h1>
            <p className="detail-price">{product.price.toFixed(2)}</p>
            <p className="detail-description">{product.description}</p>
            <p className="detail-brand">{product.productBrand}</p>

            {basketError && !basketLoading && (
              <div className="inline-error" role="alert">
                <p>{basketError}</p>
                <button className="text-button" onClick={retryBasket} type="button">
                  Retry bag connection
                </button>
              </div>
            )}
            {addError && <p className="inline-error" role="alert">{addError}</p>}
            {added && (
              <p className="inline-success" role="status">
                Added to your bag. <a href="/basket">View bag</a>
              </p>
            )}
            <button
              className="button button--dark detail-add-button"
              disabled={basketLoading || saving || basketError !== null}
              onClick={handleAddToBag}
              type="button"
            >
              {saving ? "Updating bag..." : "Add to bag"} <Icon name="arrow-right" />
            </button>
            <a className="detail-bag-link" href="/basket">View your bag</a>
          </div>
        </div>
      ) : null}
    </section>
  );
}
