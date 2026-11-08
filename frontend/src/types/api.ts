export interface ProductDto {
  id: number;
  name: string;
  description: string;
  PictureUrl: string;
  price: number;
  productBrand: string;
  productType: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  puctureUrl: string;
  price: number;
  brandId: number;
  typeId: number;
}

export interface BrandDto {
  id: number;
  name: string;
}

export interface TypeDto {
  id: number;
  name: string;
}

export interface ProductQueryParams {
  pageIndex?: number;
  pageSize?: number;
  brandId?: number;
  typeId?: number;
  search?: string;
  sort?: "NameAsc" | "NameDesc" | "PriceAsc" | "PriceDesc";
}

export interface PaginatedResult<T> {
  pageIndex: number;
  pageSize: number;
  count: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  data: T[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  displayName: string;
  phoneNumber: string;
}

export interface UserResponse {
  email: string;
  displayName: string;
  token: string;
}

export interface BasketItemDto {
  id: number;
  productName: string;
  pictureUrl: string;
  price: number;
  quantity: number;
}

export interface CustomerBasketDto {
  id: string;
  version: number;
  items: BasketItemDto[];
}

export interface SaveBasketRequestDto {
  id: string;
  expectedVersion: number;
  items: BasketItemDto[];
}

export interface MergeBasketRequestDto {
  guestBasketId: string;
  guestSecret: string;
}

export interface AddressDto {
  firstName: string;
  lastName: string;
  city: string;
  country: string;
  street: string;
}

export interface OrderItemDto {
  productId: number;
  productName: string;
  pictureUrl: string;
  price: number;
  quantity: number;
}

export interface OrderDto {
  id: string;
  userEmail: string;
  orderStatus: string;
  paymentStatus: string | null;
  orderDate: string;
  subTotal: number;
  total: number;
  address: AddressDto;
  deliveryMethod: string;
  items: OrderItemDto[];
}

export interface DeliveryMethodDto {
  id: number;
  shortName: string;
  description: string;
  deliveryTime: string;
  price: number;
}

export interface CreateOrderRequest {
  basketId: string;
  customerEmail: string;
  deliveryMethodId: number;
  shippingAddress: AddressDto;
  paymentIntentId: string | null;
}

export interface CreatePaymentIntentRequest {
  basketId: string;
  deliveryMethodId: number;
}

export interface PaymentIntentDto {
  clientSecret: string;
  paymentIntentId: string;
  amount: number;
  currency: string;
}
