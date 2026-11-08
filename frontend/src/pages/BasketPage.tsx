import { CollectionState } from "../components/ui/CollectionState";
import { Icon } from "../components/ui/Icon";
import { useBasket } from "../contexts/BasketContext";
import { getProductImageUrl } from "../data/fashionImages";

export function BasketPage() {
  const {
    basket,
    clearBasket,
    error,
    itemCount,
    loading,
    removeProduct,
    retry,
    saving,
    setQuantity,
  } = useBasket();

  if (loading) {
    return (
      <section className="basket-page page-shell" aria-labelledby="basket-heading">
        <h1 id="basket-heading">Your bag</h1>
        <CollectionState kind="loading" message="Loading your bag..." />
      </section>
    );
  }

  if (error || !basket) {
    return (
      <section className="basket-page page-shell" aria-labelledby="basket-heading">
        <h1 id="basket-heading">Your bag</h1>
        <CollectionState
          kind="error"
          message={error ?? "Your bag could not be loaded."}
          onRetry={retry}
        />
      </section>
    );
  }

  const subtotal = basket.items.reduce(
    (total, item) => total + item.price * item.quantity,
    0,
  );

  return (
    <section className="basket-page page-shell" aria-labelledby="basket-heading">
      <a className="back-link" href="/#shop">
        <Icon name="arrow-left" /> Continue shopping
      </a>
      <div className="basket-heading-row">
        <div>
          <span className="eyebrow">Your Forma selection</span>
          <h1 id="basket-heading">Your bag</h1>
        </div>
        <span className="basket-heading-count">
          {itemCount} {itemCount === 1 ? "item" : "items"}
        </span>
      </div>

      {error && (
        <div className="inline-error" role="alert">
          <p>{error}</p>
          <button className="text-button" onClick={retry} type="button">Retry</button>
        </div>
      )}

      {basket.items.length === 0 ? (
        <div className="basket-empty">
          <span className="state-icon"><Icon name="bag" /></span>
          <h2>Your bag is waiting for something lovely.</h2>
          <p>Explore the collection and find pieces made for your everyday.</p>
          <a className="button button--dark" href="/#shop">Explore the collection</a>
        </div>
      ) : (
        <div className="basket-layout">
          <div className="basket-items">
            {basket.items.map((item) => {
              const imageProduct = { name: item.productName, productType: "" };
              const source = getProductImageUrl(item.pictureUrl, imageProduct);
              const imageIsFallback = source !== null && source !== item.pictureUrl;

              return (
                <article className="basket-item" key={item.id}>
                  <a className="basket-item-image" href={`/products/${item.id}`}>
                    {source ? (
                      <img
                        alt={imageIsFallback ? `Style reference for ${item.productName}` : item.productName}
                        src={source}
                      />
                    ) : (
                      <span>Image unavailable</span>
                    )}
                  </a>
                  <div className="basket-item-info">
                    <h2><a href={`/products/${item.id}`}>{item.productName}</a></h2>
                    <span className="basket-item-unit-price">{item.price.toFixed(2)} each</span>
                    <div className="basket-item-actions">
                      <div aria-label={`Quantity for ${item.productName}`} className="quantity-control">
                        <button
                          aria-label={`Decrease ${item.productName} quantity`}
                          disabled={saving || item.quantity <= 1}
                          onClick={() => void setQuantity(item.id, item.quantity - 1)}
                          type="button"
                        >
                          −
                        </button>
                        <span aria-live="polite">{item.quantity}</span>
                        <button
                          aria-label={`Increase ${item.productName} quantity`}
                          disabled={saving}
                          onClick={() => void setQuantity(item.id, item.quantity + 1)}
                          type="button"
                        >
                          +
                        </button>
                      </div>
                      <button
                        className="basket-remove"
                        disabled={saving}
                        onClick={() => void removeProduct(item.id)}
                        type="button"
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                  <strong className="basket-item-total">
                    {(item.price * item.quantity).toFixed(2)}
                  </strong>
                </article>
              );
            })}
          </div>

          <aside className="basket-summary" aria-label="Bag summary">
            <h2>Summary</h2>
            <div><span>Items ({itemCount})</span><span>{subtotal.toFixed(2)}</span></div>
            <div className="basket-subtotal"><strong>Subtotal</strong><strong>{subtotal.toFixed(2)}</strong></div>
            <p>Shipping and payment are not included in this step.</p>
            <a className="button button--dark" href="/checkout">Continue to checkout</a>
            <button
              className="basket-clear"
              disabled={saving}
              onClick={() => void clearBasket()}
              type="button"
            >
              Clear bag
            </button>
          </aside>
        </div>
      )}
    </section>
  );
}
