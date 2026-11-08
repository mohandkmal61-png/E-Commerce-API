import { Footer } from "./components/layout/Footer";
import { Header } from "./components/layout/Header";
import { AuthProvider } from "./contexts/AuthContext";
import { BasketProvider } from "./contexts/BasketContext";
import { AuthPage } from "./pages/AuthPage";
import { BasketPage } from "./pages/BasketPage";
import { CheckoutPage } from "./pages/CheckoutPage";
import { HomePage } from "./pages/HomePage";
import { ProductDetailsPage } from "./pages/ProductDetailsPage";

export default function App() {
  const productMatch = /^\/products\/(\d+)\/?$/.exec(window.location.pathname);
  const productId = productMatch ? Number(productMatch[1]) : null;
  const page = /^\/basket\/?$/.test(window.location.pathname)
    ? <BasketPage />
    : /^\/checkout\/?$/.test(window.location.pathname)
      ? <CheckoutPage />
      : /^\/login\/?$/.test(window.location.pathname)
        ? <AuthPage />
    : productId !== null && Number.isSafeInteger(productId) && productId > 0
      ? <ProductDetailsPage productId={productId} />
      : <HomePage />;

  return (
    <AuthProvider>
      <BasketProvider>
        <Header />
        <main>{page}</main>
        <Footer />
      </BasketProvider>
    </AuthProvider>
  );
}
