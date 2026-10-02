using E_Commerse.Sharde.Enums;
using ECommerce.Domin.Entitie.ProductModule;

namespace ECommerce.Services.Spsification
{
    public class ProductSpesification : BaseSpesification<Product, int>
    {
        public ProductSpesification(
            int? brandId = null,
            int? typeId = null,
            string? search = null,
            ProductSortOption? sort = null,
            int? pageIndex = null,
            int? pageSize = null)
        {
            addInclude(product => product.ProductBrands);
            addInclude(product => product.ProductTypes);

            if (brandId.HasValue)
                AddCriteria(product => product.BrandId == brandId.Value);

            if (typeId.HasValue)
                AddCriteria(product => product.TypeId == typeId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                AddCriteria(product =>
                    product.Name.Contains(searchTerm) ||
                    product.Description.Contains(searchTerm));
            }

            ApplySorting(sort);

            if (pageIndex.HasValue && pageSize.HasValue)
                ApplyPaging(pageIndex.Value * pageSize.Value, pageSize.Value);
        }

        private void ApplySorting(ProductSortOption? sort)
        {
            switch (sort)
            {
                case ProductSortOption.NameDesc:
                    AddOrderByDescending(product => product.Name);
                    break;
                case ProductSortOption.PriceAsc:
                    AddOrderBy(product => product.Price);
                    break;
                case ProductSortOption.PriceDesc:
                    AddOrderByDescending(product => product.Price);
                    break;
                case ProductSortOption.NameAsc:
                case null:
                    AddOrderBy(product => product.Name);
                    break;
            }
        }
    }
}
