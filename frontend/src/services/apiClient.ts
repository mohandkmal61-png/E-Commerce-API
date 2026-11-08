import type {
  BrandDto,
  CustomerBasketDto,
  CreateProductDto,
  PaginatedResult,
  ProductDto,
  ProductQueryParams,
  CreateOrderRequest,
  CreatePaymentIntentRequest,
  DeliveryMethodDto,
  LoginRequest,
  MergeBasketRequestDto,
  OrderDto,
  PaymentIntentDto,
  RegisterRequest,
  TypeDto,
  UserResponse,
} from "../types/api";
import { AUTH_EXPIRED_EVENT, clearStoredUser, getAccessToken } from "./authStorage";

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? "")
  .trim()
  .replace(/\/+$/, "");

function buildRequestUrl(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const baseEndsInApi = /\/api$/i.test(apiBaseUrl);
  const pathStartsInApi = /^\/api(?:\/|$)/i.test(normalizedPath);

  if (baseEndsInApi && pathStartsInApi) {
    return `${apiBaseUrl}${normalizedPath.slice(4)}`;
  }

  return `${apiBaseUrl}${normalizedPath}`;
}

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface RequestOptions extends RequestInit {
  token?: string | null;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { token, headers: requestHeaders, ...init } = options;
  const accessToken = token === undefined ? getAccessToken() : token;
  const headers = new Headers(requestHeaders);
  headers.set("Accept", "application/json");

  if (init.body !== undefined && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`);
  }

  let response: Response;
  try {
    response = await fetch(buildRequestUrl(path), { ...init, headers });
  } catch (error: unknown) {
    if (error instanceof Error) {
      throw new ApiError(error.message || "The API could not be reached.", 0);
    }
    throw new ApiError("The API could not be reached.", 0);
  }

  if (!response.ok) {
    const responseText = await response.text();
    let body: unknown = responseText;
    if (responseText) {
      try {
        body = JSON.parse(responseText) as unknown;
      } catch {
        body = responseText;
      }
    }
    if (
      response.status === 401 &&
      accessToken &&
      !/^\/api\/Auth\//i.test(path)
    ) {
      clearStoredUser();
      window.dispatchEvent(new Event(AUTH_EXPIRED_EVENT));
    }
    throw new ApiError(getErrorMessage(body, response.statusText), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function getErrorMessage(body: unknown, fallback: string): string {
  if (typeof body === "string") {
    return body;
  }

  if (body && typeof body === "object") {
    const candidate = body as Record<string, unknown>;
    if (typeof candidate.message === "string") {
      return candidate.message;
    }
    if (typeof candidate.description === "string") {
      return candidate.description;
    }
    if (typeof candidate.title === "string") {
      return candidate.title;
    }
    if (Array.isArray(candidate.errors)) {
      const descriptions = candidate.errors
        .map((error) =>
          error && typeof error === "object"
            ? (error as Record<string, unknown>).description
            : null,
        )
        .filter((description): description is string => typeof description === "string");
      if (descriptions.length > 0) {
        return descriptions.join(" ");
      }
    }
  }

  return fallback || "The request failed.";
}

function toQueryString(params: ProductQueryParams): string {
  const searchParams = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.set(key, String(value));
    }
  }

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

export const apiClient = {
  login(credentials: LoginRequest) {
    return request<UserResponse>("/api/Auth/login", {
      method: "POST",
      body: JSON.stringify(credentials),
      token: null,
    });
  },

  register(credentials: RegisterRequest) {
    return request<UserResponse>("/api/Auth/register", {
      method: "POST",
      body: JSON.stringify(credentials),
      token: null,
    });
  },

  getProducts(params: ProductQueryParams = {}, token?: string) {
    return request<PaginatedResult<ProductDto>>(
      `/api/ProductControl${toQueryString(params)}`,
      { token },
    );
  },

  getFilteredProducts(params: ProductQueryParams, token?: string) {
    return request<PaginatedResult<ProductDto>>(
      `/api/ProductControl/filter${toQueryString(params)}`,
      { token },
    );
  },

  getProduct(id: number, token?: string) {
    return request<ProductDto>(`/api/ProductControl/${id}`, { token });
  },

  getBasket(guest?: { id: string; secret: string }) {
    if (!guest) {
      return request<CustomerBasketDto>("/api/BasketControl");
    }

    const query = new URLSearchParams({ guestBasketId: guest.id });
    return request<CustomerBasketDto>(`/api/BasketControl?${query}`, {
      headers: { "X-Basket-Guest-Secret": guest.secret },
    });
  },

  saveBasket(
    basket: CustomerBasketDto,
    expectedVersion: number,
    guestSecret?: string,
  ) {
    return request<CustomerBasketDto>("/api/BasketControl", {
      method: "POST",
      body: JSON.stringify({
        id: basket.id,
        expectedVersion,
        items: basket.items,
      }),
      ...(guestSecret ? { headers: { "X-Basket-Guest-Secret": guestSecret } } : {}),
    });
  },

  mergeBasket(requestBody: MergeBasketRequestDto) {
    return request<CustomerBasketDto>("/api/BasketControl/merge", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  getDeliveryMethods() {
    return request<DeliveryMethodDto[]>("/api/OrderControl/delivery-methods");
  },

  createPaymentIntent(requestBody: CreatePaymentIntentRequest) {
    return request<PaymentIntentDto>("/api/payment/intent", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  createOrder(requestBody: CreateOrderRequest) {
    return request<OrderDto>("/api/OrderControl", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  },

  getBrands(token?: string) {
    return request<BrandDto[]>("/api/ProductControl/brands", { token });
  },

  getTypes(token?: string) {
    return request<TypeDto[]>("/api/ProductControl/types", { token });
  },

  createProduct(product: CreateProductDto, token?: string) {
    return request<ProductDto>("/api/ProductControl", {
      method: "POST",
      body: JSON.stringify(product),
      token,
    });
  },
};
