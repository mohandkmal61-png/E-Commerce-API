using E_Commerse.Sharde.DTOS.Pagination;
using E_Commerse.Sharde.Enums;

namespace E_Commerse.Sharde.DTOS.ProdactDtos
{
    public class ProductQueryParams : PaginationParams
    {
        public int? BrandId { get; set; }
        public int? TypeId { get; set; }
        public string? Search { get; set; }
        public ProductSortOption? Sort { get; set; }
    }
}
