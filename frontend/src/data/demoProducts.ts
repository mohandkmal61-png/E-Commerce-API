export interface DemoProduct {
  id: number;
  name: string;
  category: string;
  price: number;
  image: string;
  tag?: string;
  rating: string;
  reviews: number;
  tone: string;
}

export const demoProducts: DemoProduct[] = [
  {
    id: 1,
    name: "Studio Form Chair",
    category: "Furniture",
    price: 248,
    image:
      "https://images.unsplash.com/photo-1503602642458-232111445657?auto=format&fit=crop&w=900&q=85",
    tag: "Bestseller",
    rating: "4.9",
    reviews: 128,
    tone: "tone-sand",
  },
  {
    id: 2,
    name: "Arc Table Lamp",
    category: "Lighting",
    price: 129,
    image:
      "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?auto=format&fit=crop&w=900&q=85",
    tag: "New arrival",
    rating: "4.8",
    reviews: 84,
    tone: "tone-olive",
  },
  {
    id: 3,
    name: "Everyday Carry Tote",
    category: "Accessories",
    price: 68,
    image:
      "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=900&q=85",
    rating: "4.9",
    reviews: 216,
    tone: "tone-blush",
  },
  {
    id: 4,
    name: "Form Ceramic Set",
    category: "Home objects",
    price: 54,
    image:
      "https://images.unsplash.com/photo-1490312278390-ab64016e0aa9?auto=format&fit=crop&w=900&q=85",
    tag: "Low stock",
    rating: "4.7",
    reviews: 63,
    tone: "tone-clay",
  },
];

export const demoCategories = [
  {
    name: "Furniture",
    count: "24 pieces",
    image:
      "https://images.unsplash.com/photo-1494438639946-1ebd1d20bf85?auto=format&fit=crop&w=900&q=85",
    className: "category-card--furniture",
  },
  {
    name: "Lighting",
    count: "18 pieces",
    image:
      "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?auto=format&fit=crop&w=900&q=85",
    className: "category-card--lighting",
  },
  {
    name: "Home objects",
    count: "32 pieces",
    image:
      "https://images.unsplash.com/photo-1490312278390-ab64016e0aa9?auto=format&fit=crop&w=900&q=85",
    className: "category-card--objects",
  },
  {
    name: "Accessories",
    count: "16 pieces",
    image:
      "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=900&q=85",
    className: "category-card--accessories",
  },
];
