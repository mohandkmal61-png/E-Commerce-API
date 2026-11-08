import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { useAuth } from "./AuthContext";
import { ApiError, apiClient } from "../services/apiClient";
import type { BasketItemDto, CustomerBasketDto, ProductDto } from "../types/api";

const BASKET_ID_KEY = "forma.basketId";
const GUEST_SECRET_KEY = "forma.guestSecret";
const BASKET_ID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const GUEST_SECRET_PATTERN = /^[0-9a-f]{64}$/i;

interface GuestCredentials {
  id: string;
  secret: string;
}

interface BasketContextValue {
  basket: CustomerBasketDto | null;
  loading: boolean;
  saving: boolean;
  error: string | null;
  itemCount: number;
  isAuthenticated: boolean;
  addProduct: (product: ProductDto) => Promise<boolean>;
  setQuantity: (productId: number, quantity: number) => Promise<boolean>;
  removeProduct: (productId: number) => Promise<boolean>;
  clearBasket: () => Promise<boolean>;
  reload: () => void;
  retry: () => void;
}

const BasketContext = createContext<BasketContextValue | undefined>(undefined);

function getGuestCredentials(): GuestCredentials {
  const storedId = window.localStorage.getItem(BASKET_ID_KEY);
  const storedSecret = window.localStorage.getItem(GUEST_SECRET_KEY);
  if (
    storedId &&
    BASKET_ID_PATTERN.test(storedId) &&
    storedSecret &&
    GUEST_SECRET_PATTERN.test(storedSecret)
  ) {
    return { id: storedId, secret: storedSecret };
  }

  const bytes = window.crypto.getRandomValues(new Uint8Array(32));
  const credentials = {
    id: window.crypto.randomUUID(),
    secret: Array.from(bytes, (byte) => byte.toString(16).padStart(2, "0")).join(""),
  };
  window.localStorage.setItem(BASKET_ID_KEY, credentials.id);
  window.localStorage.setItem(GUEST_SECRET_KEY, credentials.secret);
  return credentials;
}

function clearGuestCredentials(): void {
  window.localStorage.removeItem(BASKET_ID_KEY);
  window.localStorage.removeItem(GUEST_SECRET_KEY);
}

function isBasketResponse(value: unknown): value is CustomerBasketDto {
  if (!value || typeof value !== "object") {
    return false;
  }

  const basket = value as Partial<CustomerBasketDto>;
  return (
    typeof basket.id === "string" &&
    typeof basket.version === "number" &&
    Array.isArray(basket.items)
  );
}

function getErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof Error && error.message) {
    return error.message;
  }
  return fallback;
}

function isVersionConflict(error: unknown): boolean {
  return (
    error instanceof ApiError &&
    (error.status === 409 ||
      /conflict|modified|version mismatch/i.test(error.message))
  );
}

interface BasketProviderProps {
  children: ReactNode;
}

export function BasketProvider({ children }: BasketProviderProps) {
  const { user, ready: authReady } = useAuth();
  const ownerKey = user?.token ?? null;
  const [basket, setBasket] = useState<CustomerBasketDto | null>(null);
  const [loadedOwner, setLoadedOwner] = useState<string | null | undefined>();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [requestId, setRequestId] = useState(0);
  const basketRef = useRef<CustomerBasketDto | null>(null);
  const ownerRef = useRef<string | null>(ownerKey);
  const queueRef = useRef<Promise<void>>(Promise.resolve());

  ownerRef.current = ownerKey;

  const commitBasket = useCallback((nextBasket: CustomerBasketDto | null) => {
    basketRef.current = nextBasket;
    setBasket(nextBasket);
  }, []);

  useEffect(() => {
    if (!authReady) {
      return;
    }

    let isCurrentRequest = true;
    const loadBasket = async (): Promise<{
      basket: CustomerBasketDto;
      mergeError: string | null;
    }> => {
      await queueRef.current;
      if (!isCurrentRequest) {
        throw new Error("Basket loading was cancelled.");
      }

      if (user) {
        let mergeError: string | null = null;
        const storedGuestId = window.localStorage.getItem(BASKET_ID_KEY);
        const storedGuestSecret = window.localStorage.getItem(GUEST_SECRET_KEY);

        if (storedGuestId && storedGuestSecret) {
          try {
            const mergedBasket = await apiClient.mergeBasket({
              guestBasketId: storedGuestId,
              guestSecret: storedGuestSecret,
            });
            if (!isBasketResponse(mergedBasket)) {
              throw new Error("The server returned an invalid basket after merging.");
            }
            clearGuestCredentials();
            return { basket: mergedBasket, mergeError: null };
          } catch (mergeRequestError: unknown) {
            mergeError = `Your account is signed in, but the guest bag could not be merged. Retry to preserve and merge it. ${getErrorMessage(mergeRequestError, "Please try again.")}`;
          }
        }

        try {
          const userBasket = await apiClient.getBasket();
          if (!isBasketResponse(userBasket)) {
            throw new Error("The server returned an invalid user basket.");
          }
          return { basket: userBasket, mergeError };
        } catch (requestError: unknown) {
          throw new Error(
            mergeError ??
              getErrorMessage(requestError, "The account bag could not be loaded."),
          );
        }
      }

      const guest = getGuestCredentials();
      try {
        const guestBasket = await apiClient.getBasket(guest);
        if (!isBasketResponse(guestBasket)) {
          throw new Error("The server returned an invalid guest basket.");
        }
        return { basket: guestBasket, mergeError: null };
      } catch (requestError: unknown) {
        if (!(requestError instanceof ApiError) || requestError.status !== 404) {
          throw requestError;
        }

        const createdBasket = await apiClient.saveBasket(
          { id: guest.id, version: 0, items: [] },
          0,
          guest.secret,
        );
        if (!isBasketResponse(createdBasket)) {
          throw new Error("The server returned an invalid guest basket.");
        }
        return { basket: createdBasket, mergeError: null };
      }
    };

    commitBasket(null);
    setLoadedOwner(undefined);
    setLoading(true);
    setError(null);
    loadBasket()
      .then((result) => {
        if (!isCurrentRequest) {
          return;
        }
        commitBasket(result.basket);
        setLoadedOwner(ownerKey);
        setError(result.mergeError);
      })
      .catch((requestError: unknown) => {
        if (isCurrentRequest) {
          commitBasket(null);
          setLoadedOwner(ownerKey);
          setError(getErrorMessage(requestError, "The shopping bag could not be loaded."));
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
  }, [authReady, user, ownerKey, requestId, commitBasket]);

  const mutateBasket = useCallback(
    (updateItems: (current: BasketItemDto[]) => BasketItemDto[]) => {
      const operation = queueRef.current.then(async () => {
        const currentBasket = basketRef.current;
        const operationOwner = ownerRef.current;
        if (!currentBasket || loadedOwner !== operationOwner) {
          return false;
        }

        setSaving(true);
        setError(null);
        try {
          const nextItems = updateItems(currentBasket.items);
          const guest = operationOwner === null ? getGuestCredentials() : null;
          const optimisticBasket = { ...currentBasket, items: nextItems };
          if (ownerRef.current === operationOwner) {
            commitBasket(optimisticBasket);
          }
          const savedBasket = await apiClient.saveBasket(
            optimisticBasket,
            currentBasket.version,
            guest?.secret,
          );
          if (!isBasketResponse(savedBasket)) {
            throw new Error("The server returned an invalid basket after saving.");
          }
          if (ownerRef.current === operationOwner) {
            commitBasket(savedBasket);
          }
          return true;
        } catch (requestError: unknown) {
          if (isVersionConflict(requestError)) {
            let refreshError: unknown;
            try {
              const refreshedBasket =
                operationOwner === null
                  ? await apiClient.getBasket(getGuestCredentials())
                  : await apiClient.getBasket();
              if (!isBasketResponse(refreshedBasket)) {
                throw new Error("The server returned an invalid basket after refreshing.");
              }
              if (ownerRef.current === operationOwner) {
                commitBasket(refreshedBasket);
              }
            } catch (error: unknown) {
              refreshError = error;
            }
            if (refreshError) {
              setError(
                `The bag changed in another request and could not be refreshed. ${getErrorMessage(refreshError, "Retry to load the latest version.")}`,
              );
            } else {
              setError("The bag changed in another request. It has been refreshed; please retry your change.");
            }
          } else {
            if (ownerRef.current === operationOwner) {
              commitBasket(currentBasket);
            }
            setError(getErrorMessage(requestError, "The shopping bag could not be updated."));
          }
          return false;
        } finally {
          setSaving(false);
        }
      });

      queueRef.current = operation.then(() => undefined, () => undefined);
      return operation;
    },
    [commitBasket, loadedOwner],
  );

  const addProduct = useCallback(
    (product: ProductDto) =>
      mutateBasket((items) => {
        const existingItem = items.find((item) => item.id === product.id);
        return existingItem
          ? items.map((item) =>
              item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item,
            )
          : [
              ...items,
              {
                id: product.id,
                productName: product.name,
                pictureUrl: product.PictureUrl,
                price: product.price,
                quantity: 1,
              },
            ];
      }),
    [mutateBasket],
  );

  const setQuantity = useCallback(
    (productId: number, quantity: number) =>
      mutateBasket((items) =>
        items.map((item) =>
          item.id === productId ? { ...item, quantity: Math.max(1, quantity) } : item,
        ),
      ),
    [mutateBasket],
  );

  const removeProduct = useCallback(
    (productId: number) =>
      mutateBasket((items) => items.filter((item) => item.id !== productId)),
    [mutateBasket],
  );

  const clearBasket = useCallback(() => mutateBasket(() => []), [mutateBasket]);
  const retry = useCallback(() => setRequestId((attempt) => attempt + 1), []);
  const reload = useCallback(() => setRequestId((attempt) => attempt + 1), []);

  const currentBasket = loadedOwner === ownerKey ? basket : null;
  const value = useMemo<BasketContextValue>(
    () => ({
      basket: currentBasket,
      loading: !authReady || loading || loadedOwner !== ownerKey,
      saving,
      error,
      itemCount: currentBasket?.items.reduce((count, item) => count + item.quantity, 0) ?? 0,
      isAuthenticated: user !== null,
      addProduct,
      setQuantity,
      removeProduct,
      clearBasket,
      reload,
      retry,
    }),
    [
      currentBasket,
      authReady,
      loading,
      loadedOwner,
      ownerKey,
      saving,
      error,
      user,
      addProduct,
      setQuantity,
      removeProduct,
      clearBasket,
      reload,
      retry,
    ],
  );

  return <BasketContext.Provider value={value}>{children}</BasketContext.Provider>;
}

export function useBasket(): BasketContextValue {
  const context = useContext(BasketContext);
  if (!context) {
    throw new Error("useBasket must be used within a BasketProvider.");
  }
  return context;
}
