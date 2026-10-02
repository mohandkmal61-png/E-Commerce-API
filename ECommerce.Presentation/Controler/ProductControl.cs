using ECommerce.Presentation.Extensions;
using ECommerce.ServicesAbstraction;
using E_Commerse.Sharde.DTOS.ProdactDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Controler
{
   // [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductControl : ControllerBase
    {
        private readonly IProductServise _productServise;

        public ProductControl(IProductServise productServise)
        {
            _productServise = productServise;
        }

            
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto productDto)
        {
            var result = await _productServise.AddProductAsync(productDto);
            return result.ToActionResult(this);
        }

        #region Get All Products
        [Authorize(AuthenticationSchemes = "Bearer")] 
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParams queryParams)
        {
            if (queryParams.PageIndex < 0 || queryParams.PageSize <= 0)
                return BadRequest("pageIndex must be >= 0 and pageSize must be > 0.");

            var result = await _productServise.GetAllProductsAsync(queryParams);
            return result.ToActionResult(this);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetProductsByFilter([FromQuery] ProductQueryParams queryParams)
        {
            if (!queryParams.BrandId.HasValue && !queryParams.TypeId.HasValue && string.IsNullOrWhiteSpace(queryParams.Search))
                return BadRequest("Please provide brandId, typeId, or search.");

            if (queryParams.PageIndex < 0 || queryParams.PageSize <= 0)
                return BadRequest("pageIndex must be >= 0 and pageSize must be > 0.");

            var result = await _productServise.GetProductsByFilterAsync(queryParams);
            return result.ToActionResult(this);
        }

        #endregion

        #region  Get Product By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var result = await _productServise.GetProdactByIdAsync(id);
            return result.ToActionResult(this);
        }

        #endregion

        #region Get All Brands
        [HttpGet("brands")]
        public async Task<IActionResult> GetAllBrands()
        {
            var result = await _productServise.GetAllBrandsAsync();
            return result.ToActionResult(this);
        }
        #endregion

        #region Get All Types
        [HttpGet("types")]
        public async Task<IActionResult> GetAllTypes()
        {
            var result = await _productServise.GetAllTypesAsync();
            return result.ToActionResult(this);
        }
        #endregion
    }
}
