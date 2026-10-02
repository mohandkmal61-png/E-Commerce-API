using AutoMapper;
using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.ProdactDtos;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.ProductModule;
using ECommerce.Services.Spsification;
using ECommerce.ServicesAbstraction;

namespace ECommerce.Services
{
    public class ProductServise : IProductServise
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductServise(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<ProdactDtos>> AddProductAsync(CreateProductDto productDto)
        {
            if (string.IsNullOrWhiteSpace(productDto.Name))
                return Result<ProdactDtos>.fail(Error.Validation("ProductNameRequired", "Product name is required."));

            if (productDto.Price < 0)
                return Result<ProdactDtos>.fail(Error.Validation("InvalidPrice", "Product price cannot be negative."));

            var brand = await _unitOfWork.GetRepository<ProductBrand, int>().GetByIdAsync(productDto.BrandId);
            if (brand is null)
                return Result<ProdactDtos>.fail(Error.Validation("BrandNotFound", "The selected brand was not found."));

            var type = await _unitOfWork.GetRepository<ProductType, int>().GetByIdAsync(productDto.TypeId);
            if (type is null)
                return Result<ProdactDtos>.fail(Error.Validation("TypeNotFound", "The selected type was not found."));

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.GetRepository<Product, int>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            product.ProductBrands = brand;
            product.ProductTypes = type;
            return Result<ProdactDtos>.ok(_mapper.Map<ProdactDtos>(product));
        }

        public async Task<Result<IEnumerable<BrandDtos>>> GetAllBrandsAsync()
        {
            var brands = await _unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync();
            return Result<IEnumerable<BrandDtos>>.ok(_mapper.Map<IEnumerable<BrandDtos>>(brands));
        }

        public Task<Result<PaginatedResult<ProdactDtos>>> GetAllProductsAsync(ProductQueryParams queryParams)
            => GetProductsPaginatedAsync(queryParams);

        public Task<Result<PaginatedResult<ProdactDtos>>> GetProductsByFilterAsync(ProductQueryParams queryParams)
            => GetProductsPaginatedAsync(queryParams);

        public async Task<Result<IEnumerable<TypeDtos>>> GetAllTypesAsync()
        {
            var types = await _unitOfWork.GetRepository<ProductType, int>().GetAllAsync();
            return Result<IEnumerable<TypeDtos>>.ok(_mapper.Map<IEnumerable<TypeDtos>>(types));
        }

        public async Task<Result<ProdactDtos>> GetProdactByIdAsync(int id)
        {
            var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(id);
            if (product is null)
                return Result<ProdactDtos>.fail(Error.NotFound($"Product with id {id} was not found."));

            return Result<ProdactDtos>.ok(_mapper.Map<ProdactDtos>(product));
        }

        private async Task<Result<PaginatedResult<ProdactDtos>>> GetProductsPaginatedAsync(ProductQueryParams queryParams)
        {
            var repository = _unitOfWork.GetRepository<Product, int>();
            var specification = new ProductSpesification(
                queryParams.BrandId,
                queryParams.TypeId,
                queryParams.Search,
                queryParams.Sort,
                queryParams.PageIndex,
                queryParams.PageSize);

            var count = await repository.CountAsync(specification);
            var products = await repository.GetAll(specification);

            var paginated = PaginatedResult<ProdactDtos>.Create(
                queryParams.PageIndex,
                queryParams.PageSize,
                count,
                _mapper.Map<IEnumerable<ProdactDtos>>(products));

            return Result<PaginatedResult<ProdactDtos>>.ok(paginated);
        }
    }
}
