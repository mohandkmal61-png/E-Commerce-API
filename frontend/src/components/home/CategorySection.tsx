import type { TypeDto } from "../../types/api";
import { getCategoryImage } from "../../data/fashionImages";
import { CollectionState } from "../ui/CollectionState";
import { Icon } from "../ui/Icon";

interface CategorySectionProps {
  types: TypeDto[];
  selectedTypeId: number | null;
  loading: boolean;
  error: string | null;
  onSelectType: (typeId: number) => void;
  onRetry: () => void;
}

export function CategorySection({
  types,
  selectedTypeId,
  loading,
  error,
  onSelectType,
  onRetry,
}: CategorySectionProps) {
  return (
    <section aria-labelledby="categories-heading" className="category-section page-shell" id="categories">
      <div className="section-heading section-heading--split">
        <div>
          <span className="eyebrow">Pieces for every day</span>
          <h2 id="categories-heading">Find your kind of <em>everyday.</em></h2>
        </div>
        <a className="text-button" href="#shop">
          Browse all categories <Icon name="arrow-right" />
        </a>
      </div>
      {loading ? (
        <CollectionState kind="loading" message="Loading product categories..." />
      ) : error ? (
        <CollectionState kind="error" message={error} onRetry={onRetry} />
      ) : types.length === 0 ? (
        <CollectionState
          kind="empty"
          message="There are no product categories available right now."
        />
      ) : (
        <div className="category-grid">
          {types.map((type, index) => (
            <button
              aria-pressed={selectedTypeId === type.id}
              className={`category-card category-card--${(index % 4) + 1}${selectedTypeId === type.id ? " category-card--selected" : ""}`}
              key={type.id}
              onClick={() => onSelectType(type.id)}
              type="button"
            >
              <img alt="" loading="lazy" src={getCategoryImage(type.name)} />
              <span className="category-overlay" />
              <span className="category-index">{String(index + 1).padStart(2, "0")}</span>
              <span className="category-content">
                <span>
                  <strong>{type.name}</strong>
                  <small>Shop this category</small>
                </span>
                <span className="category-arrow">
                  <Icon name="arrow-right" />
                </span>
              </span>
            </button>
          ))}
        </div>
      )}
    </section>
  );
}
