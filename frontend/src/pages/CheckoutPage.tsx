import { useEffect, useState, type FormEvent } from "react";
import { CollectionState } from "../components/ui/CollectionState";
import { useAuth } from "../contexts/AuthContext";
import { useBasket } from "../contexts/BasketContext";
import { ApiError, apiClient } from "../services/apiClient";
import type {
  AddressDto,
  DeliveryMethodDto,
  OrderDto,
  PaymentIntentDto,
} from "../types/api";

const emptyAddress: AddressDto = {
  firstName: "",
  lastName: "",
  city: "",
  country: "",
  street: "",
};

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status === 0) {
      return "Could not connect to the API. Check the API URL and network, then retry.";
    }
    if (error.status === 401) {
      return "Your session expired. Sign in again to continue checkout.";
    }
    return error.message;
  }
  return error instanceof Error ? error.message : "Checkout could not be completed.";
}

export function CheckoutPage() {
  const { user } = useAuth();
  const {
    basket,
    error: basketError,
    loading: basketLoading,
    reload,
    retry: retryBasket,
  } = useBasket();
  const [deliveryMethods, setDeliveryMethods] = useState<DeliveryMethodDto[]>([]);
  const [deliveryMethodId, setDeliveryMethodId] = useState("");
  const [address, setAddress] = useState<AddressDto>(emptyAddress);
  const [loadingMethods, setLoadingMethods] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [paymentIntent, setPaymentIntent] = useState<PaymentIntentDto | null>(null);
  const [order, setOrder] = useState<OrderDto | null>(null);

  useEffect(() => {
    if (!user) {
      setDeliveryMethods([]);
      setLoadingMethods(false);
      return;
    }

    let active = true;
    setLoadingMethods(true);
    setError(null);
    apiClient
      .getDeliveryMethods()
      .then((methods) => {
        if (active) {
          setDeliveryMethods(methods);
          setDeliveryMethodId((current) =>
            methods.some((method) => String(method.id) === current)
              ? current
              : String(methods[0]?.id ?? ""),
          );
        }
      })
      .catch((requestError: unknown) => {
        if (active) {
          setError(getErrorMessage(requestError));
        }
      })
      .finally(() => {
        if (active) {
          setLoadingMethods(false);
        }
      });

    return () => {
      active = false;
    };
  }, [user]);

  function updateAddress(field: keyof AddressDto, value: string) {
    setAddress((current) => ({ ...current, [field]: value }));
  }

  async function handlePrepareOrder(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!basket || !user || !deliveryMethodId) {
      return;
    }

    setSubmitting(true);
    setError(null);
    setOrder(null);
    try {
      const intent =
        paymentIntent && paymentIntent.paymentIntentId
          ? paymentIntent
          : await apiClient.createPaymentIntent({
              basketId: basket.id,
              deliveryMethodId: Number(deliveryMethodId),
            });
      setPaymentIntent(intent);

      const createdOrder = await apiClient.createOrder({
        basketId: basket.id,
        customerEmail: user.email,
        deliveryMethodId: Number(deliveryMethodId),
        shippingAddress: address,
        paymentIntentId: intent.paymentIntentId,
      });
      setOrder(createdOrder);
      reload();
    } catch (requestError: unknown) {
      setError(getErrorMessage(requestError));
    } finally {
      setSubmitting(false);
    }
  }

  const selectedDeliveryMethod = deliveryMethods.find(
    (method) => method.id === Number(deliveryMethodId),
  );
  const subtotal =
    basket?.items.reduce((total, item) => total + item.price * item.quantity, 0) ?? 0;
  const total = subtotal + (selectedDeliveryMethod?.price ?? 0);

  if (!user) {
    return (
      <section className="checkout-page page-shell" aria-labelledby="checkout-heading">
        <h1 id="checkout-heading">Checkout</h1>
        <div className="checkout-notice">
          <p>Sign in to securely associate this checkout with your account.</p>
          <a className="button button--dark" href="/login?returnUrl=%2Fcheckout">Sign in</a>
        </div>
      </section>
    );
  }

  if (basketLoading) {
    return (
      <section className="checkout-page page-shell" aria-labelledby="checkout-heading">
        <h1 id="checkout-heading">Checkout</h1>
        <CollectionState kind="loading" message="Loading your account bag..." />
      </section>
    );
  }

  if (!basket || basket.items.length === 0) {
    return (
      <section className="checkout-page page-shell" aria-labelledby="checkout-heading">
        <h1 id="checkout-heading">Checkout</h1>
        <div className="checkout-notice">
          {basketError ? (
            <>
              <p>{basketError}</p>
              <button className="text-button" onClick={retryBasket} type="button">
                Retry bag sync
              </button>
            </>
          ) : (
            <p>Your bag is empty.</p>
          )}
          <a className="button button--dark" href="/#shop">Continue shopping</a>
        </div>
      </section>
    );
  }

  return (
    <section className="checkout-page page-shell" aria-labelledby="checkout-heading">
      <a className="back-link" href="/basket">Back to your bag</a>
      <div className="checkout-heading">
        <span className="eyebrow">Delivery details</span>
        <h1 id="checkout-heading">Checkout</h1>
        <p>Signed in as {user.email}</p>
      </div>

      <div className="checkout-layout">
        <form className="checkout-form" onSubmit={(event) => void handlePrepareOrder(event)}>
          <h2>Shipping address</h2>
          <div className="checkout-fields">
            <label>
              First name
              <input
                autoComplete="given-name"
                onChange={(event) => updateAddress("firstName", event.target.value)}
                required
                value={address.firstName}
              />
            </label>
            <label>
              Last name
              <input
                autoComplete="family-name"
                onChange={(event) => updateAddress("lastName", event.target.value)}
                required
                value={address.lastName}
              />
            </label>
            <label>
              Street
              <input
                autoComplete="street-address"
                onChange={(event) => updateAddress("street", event.target.value)}
                required
                value={address.street}
              />
            </label>
            <label>
              City
              <input
                autoComplete="address-level2"
                onChange={(event) => updateAddress("city", event.target.value)}
                required
                value={address.city}
              />
            </label>
            <label>
              Country
              <input
                autoComplete="country-name"
                onChange={(event) => updateAddress("country", event.target.value)}
                required
                value={address.country}
              />
            </label>
            <label>
              Delivery method
              {loadingMethods ? (
                <span>Loading delivery options...</span>
              ) : (
                <select
                  onChange={(event) => {
                    setDeliveryMethodId(event.target.value);
                    setPaymentIntent(null);
                  }}
                  required
                  value={deliveryMethodId}
                >
                  {deliveryMethods.length === 0 && <option value="">No delivery methods available</option>}
                  {deliveryMethods.map((method) => (
                    <option key={method.id} value={method.id}>
                      {method.shortName} — {method.deliveryTime} ({method.price.toFixed(2)})
                    </option>
                  ))}
                </select>
              )}
            </label>
          </div>

          <div className="checkout-notice" role="note">
            <strong>Development checkout — no payment is confirmed.</strong>
            <p>
              Continuing creates a Stripe PaymentIntent and a pending order through the API.
              No card details are requested and this page never confirms a payment. Use a
              Stripe test secret key on the backend for development.
            </p>
          </div>

          {error && <p className="inline-error" role="alert">{error}</p>}
          {basketError && (
            <div className="inline-error" role="alert">
              <p>{basketError}</p>
              <button className="text-button" onClick={retryBasket} type="button">
                Retry bag sync
              </button>
            </div>
          )}
          {order ? (
            <div className="inline-success" role="status">
              Pending order {order.id} created. No payment was collected.
              <a href="/#shop">Continue shopping</a>
            </div>
          ) : (
            <button
              className="button button--dark"
              disabled={
                submitting ||
                loadingMethods ||
                basketError !== null ||
                deliveryMethods.length === 0 ||
                basket.items.length === 0
              }
              type="submit"
            >
              {submitting ? "Preparing order..." : "Create pending order (no charge)"}
            </button>
          )}
        </form>

        <aside className="checkout-summary" aria-label="Order summary">
          <h2>Order summary</h2>
          {basket.items.map((item) => (
            <div className="checkout-summary-item" key={item.id}>
              <span>{item.productName} × {item.quantity}</span>
              <span>{(item.price * item.quantity).toFixed(2)}</span>
            </div>
          ))}
          <div><span>Subtotal</span><span>{subtotal.toFixed(2)}</span></div>
          <div>
            <span>Delivery</span>
            <span>{selectedDeliveryMethod?.price.toFixed(2) ?? "—"}</span>
          </div>
          <div className="checkout-total">
            <strong>Total</strong><strong>{total.toFixed(2)}</strong>
          </div>
          {paymentIntent && !order && (
            <p className="checkout-intent-note">
              Intent {paymentIntent.paymentIntentId} is ready. It has not been confirmed or charged.
            </p>
          )}
        </aside>
      </div>
    </section>
  );
}
