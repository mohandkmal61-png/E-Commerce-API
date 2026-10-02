using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.ProdactDtos;

namespace ECommerce.ServicesAbstraction
{
    public interface IProductServise
    {
        Task<Result<ProdactDtos>> AddProductAsync(CreateProductDto productDto);
        Task<Result<PaginatedResult<ProdactDtos>>> GetAllProductsAsync(ProductQueryParams queryParams);
        Task<Result<PaginatedResult<ProdactDtos>>> GetProductsByFilterAsync(ProductQueryParams queryParams);
        Task<Result<ProdactDtos>> GetProdactByIdAsync(int id);
        Task<Result<IEnumerable<BrandDtos>>> GetAllBrandsAsync();
        Task<Result<IEnumerable<TypeDtos>>> GetAllTypesAsync();
    }
}
