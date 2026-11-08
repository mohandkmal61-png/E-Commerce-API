import { useEffect, useState } from "react";
import { apiClient } from "../services/apiClient";
import type { TypeDto } from "../types/api";
import { CategorySection } from "../components/home/CategorySection";
import { HeroSection } from "../components/home/HeroSection";
import { ProductSection } from "../components/home/ProductSection";
import { ServiceStrip } from "../components/home/ServiceStrip";
import { StorySection } from "../components/home/StorySection";

export function HomePage() {
  const [types, setTypes] = useState<TypeDto[]>([]);
  const [selectedTypeId, setSelectedTypeId] = useState<number | null>(null);
  const [typesLoading, setTypesLoading] = useState(true);
  const [typesError, setTypesError] = useState<string | null>(null);
  const [typesRequest, setTypesRequest] = useState(0);

  useEffect(() => {
    let isCurrentRequest = true;

    setTypesLoading(true);
    setTypesError(null);

    apiClient
      .getTypes()
      .then((result) => {
        if (!isCurrentRequest) {
          return;
        }

        setTypes(result);
        setSelectedTypeId((currentTypeId) =>
          currentTypeId !== null && result.some((type) => type.id === currentTypeId)
            ? currentTypeId
            : (result[0]?.id ?? null),
        );
      })
      .catch((error: unknown) => {
        if (!isCurrentRequest) {
          return;
        }

        setTypes([]);
        setSelectedTypeId(null);
        setTypesError(
          error instanceof Error
            ? error.message
            : "An unexpected error occurred while loading product types.",
        );
      })
      .finally(() => {
        if (isCurrentRequest) {
          setTypesLoading(false);
        }
      });

    return () => {
      isCurrentRequest = false;
    };
  }, [typesRequest]);

  function retryTypes() {
    setTypesRequest((attempt) => attempt + 1);
  }

  return (
    <>
      <HeroSection />
      <ServiceStrip />
      <CategorySection
        error={typesError}
        loading={typesLoading}
        onRetry={retryTypes}
        onSelectType={setSelectedTypeId}
        selectedTypeId={selectedTypeId}
        types={types}
      />
      <ProductSection
        error={typesError}
        loadingTypes={typesLoading}
        onRetryTypes={retryTypes}
        onSelectType={setSelectedTypeId}
        selectedTypeId={selectedTypeId}
        types={types}
      />
      <StorySection />
    </>
  );
}
