import type { ProductDto } from "../types/api";

const categoryImages: Record<string, string> = {
  tops: "/images/fashion/tops.jpg",
  bottoms: "/images/fashion/denim.jpg",
  "outerwear & dresses": "/images/fashion/casual-look.jpg",
  "accessories & footwear": "/images/fashion/accessories.jpg",
};

const productImages: Record<string, string> = {
  "classic white t-shirt": "/images/fashion/tops.jpg",
  "slim fit jeans": "/images/fashion/denim.jpg",
  "denim jacket": "/images/fashion/casual-look.jpg",
  "cotton hoodie": "/images/fashion/casual-look.jpg",
  "formal blazer": "/images/fashion/casual-look.jpg",
  "summer floral dress": "/images/fashion/dress.jpg",
  "men's leather jacket": "/images/fashion/jacket.jpg",
  "casual sneakers": "/images/fashion/sneakers.jpg",
  "classic black dress pants": "/images/fashion/denim.jpg",
  "knitted sweater": "/images/fashion/casual-look.jpg",
  "men's polo shirt": "/images/fashion/tops.jpg",
  "women's trench coat": "/images/fashion/editorial-look.jpg",
  "wool scarf": "/images/fashion/accessories.jpg",
};

function normalize(value: string): string {
  return value.trim().toLocaleLowerCase("en-US");
}

export function getCategoryImage(categoryName: string): string {
  return categoryImages[normalize(categoryName)] ?? "/images/fashion/casual-look.jpg";
}

export function getProductFallbackImage(
  product: Pick<ProductDto, "name" | "productType">,
): string | null {
  const byName = productImages[normalize(product.name)];
  if (byName) {
    return byName;
  }

  return categoryImages[normalize(product.productType)] ?? null;
}

export function getProductImageUrl(
  pictureUrl: string,
  product: Pick<ProductDto, "name" | "productType">,
): string | null {
  return getRemoteProductImageUrl(pictureUrl) ?? getProductFallbackImage(product);
}

export function getRemoteProductImageUrl(pictureUrl: string): string | null {
  try {
    const url = new URL(pictureUrl);
    if (url.protocol === "http:" || url.protocol === "https:") {
      return url.toString();
    }
  } catch {
    return null;
  }

  return null;
}
