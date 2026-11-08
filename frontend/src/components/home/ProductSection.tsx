import { useEffect, useState } from "react";
import { apiClient } from "../../services/apiClient";
import type { ProductDto, TypeDto } from "../../types/api";
import { CollectionState } from "../ui/CollectionState";
import { Icon } from "../ui/Icon";
import { ProductCard } from "./ProductCard";

interface ProductSectionProps {
  types: TypeDto[];
  selectedTypeId: number | null;
  loadingTypes: boolean;
  error: string | null;
  onSelectType: (typeId: number | null) => void;
  onRetryTypes: () => void;
}

export function ProductSection({
  types,
  selectedTypeId,
  loadingTypes,
  error: typesError,
  onSelectType,
  onRetryTypes,
}: ProductSectionProps) {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [productCount, setProductCount] = useState(0);
  const [productsLoading, setProductsLoading] = useState(false);
  const [productsError, setProductsError] = useState<string | null>(null);
  const [productsRequest, setProductsRequest] = useState(0);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedSearch(search.trim());
    }, 250);

    return () => window.clearTimeout(timeout);
  }, [search]);

  useEffect(() => {
    let isCurrentRequest = true;

    if (loadingTypes) {
      setProducts([]);
      setProductCount(0);
      setProductsLoading(true);
      setProductsError(null);
      return () => {
        isCurrentRequest = false;
      };
    }

    if (typesError) {
      setProducts([]);
      setProductCount(0);
      setProductsLoading(false);
      setProductsError(typesError);
      return () => {
        isCurrentRequest = false;
      };
    }

    const selectedType = types.find((type) => type.id === selectedTypeId);

    if (!selectedType && !debouncedSearch) {
      setProducts([]);
      setProductCount(0);
      setProductsLoading(false);
      setProductsError(null);
      return () => {
        isCurrentRequest = false;
      };
    }

    setProducts([]);
    setProductCount(0);
    setProductsLoading(true);
    setProductsError(null);

    apiClient
      .getFilteredProducts({
        pageIndex: 0,
        pageSize: 12,
        ...(selectedType ? { typeId: selectedType.id } : {}),
        ...(debouncedSearch ? { search: debouncedSearch } : {}),
      })
      .then((result) => {
        if (!isCurrentRequest) {
          return;
        }

        setProducts(result.data);
        setProductCount(result.count);
      })
      .catch((requestError: unknown) => {
        if (!isCurrentRequest) {
          return;
        }

        setProducts([]);
        setProductCount(0);
        setProductsError(
          requestError instanceof Error
            ? requestError.message
            : "An unexpected error occurred while loading products.",
        );
      })
      .finally(() => {
        if (isCurrentRequest) {
          setProductsLoading(false);
        }
      });

    return () => {
      isCurrentRequest = false;
    };
  }, [
    debouncedSearch,
    productsRequest,
    selectedTypeId,
    types,
    typesError,
    loadingTypes,
  ]);

  function retryProducts() {
    if (typesError) {
      onRetryTypes();
      return;
    }

    setProductsRequest((attempt) => attempt + 1);
  }

  return (
    <section aria-labelledby="shop-heading" className="product-section page-shell" id="shop">
      <div className="section-heading section-heading--products">
        <div>
          <span className="eyebrow">The considered edit</span>
          <h2 id="shop-heading">Good things, <em>well made.</em></h2>
          <p>Explore considered pieces, selected by category or search.</p>
        </div>
        <div className="product-search">
          <Icon name="search" />
          <label className="visually-hidden" htmlFor="product-search">Search products</label>
          <input
            id="product-search"
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Find something..."
            type="search"
            value={search}
          />
          <kbd>↵</kbd>
        </div>
      </div>

      <div aria-label="Filter products by category" className="category-filters" role="group">
        <button
          aria-pressed={selectedTypeId === null}
          className={`filter-chip${selectedTypeId === null ? " filter-chip--active" : ""}`}
          onClick={() => onSelectType(null)}
          type="button"
        >
          Search all
        </button>
        {types.map((type) => (
          <button
            aria-pressed={selectedTypeId === type.id}
            className={`filter-chip${selectedTypeId === type.id ? " filter-chip--active" : ""}`}
            key={type.id}
            onClick={() => onSelectType(type.id)}
            type="button"
          >
            {type.name}
          </button>
        ))}
      </div>

      {loadingTypes || productsLoading ? (
        <CollectionState kind="loading" message="Loading products..." />
      ) : typesError || productsError ? (
        <CollectionState
          kind="error"
          message={typesError ?? productsError ?? "The product collection could not be loaded."}
          onRetry={retryProducts}
        />
      ) : products.length === 0 ? (
        <CollectionState
          kind="empty"
          message={
            selectedTypeId === null && !debouncedSearch
              ? "Choose a category or search for a product to explore the catalog."
              : undefined
          }
        />
      ) : (
        <div className="product-grid">
          {products.map((product, index) => (
            <ProductCard index={index} key={product.id} product={product} />
          ))}
        </div>
      )}

      <div className="product-section-bottom">
        <span>
          {productsLoading || productsError || typesError
            ? "Product results will appear here."
            : `Showing ${products.length} of ${productCount} products`}
        </span>
        <a className="button button--outline" href="#categories">
          Explore all pieces <Icon name="arrow-right" />
        </a>
      </div>
    </section>
  );
}
