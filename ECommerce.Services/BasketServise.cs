using AutoMapper;
using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.BasketDtos;
using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie.BasketModule;
using ECommerce.ServicesAbstraction;

namespace ECommerce.Services
{
    public class BasketServise : IBasketServise
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketServise(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        public async Task<Result<CustomerBasketDto>> GetBasketByIdAsync(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            if (basket is null)
                return Result<CustomerBasketDto>.fail(Error.NotFound($"Basket with id {id} was not found."));

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(basket));
        }

        public async Task<Result<CustomerBasketDto>> CreateOrUpdateBasketAsync(CustomerBasketDto basketDto)
        {
            if (string.IsNullOrWhiteSpace(basketDto.Id))
                return Result<CustomerBasketDto>.fail(Error.Validation("Basket id is required."));

            basketDto.Items ??= [];

            var basket = _mapper.Map<CustomerBasket>(basketDto);
            var savedBasket = await _basketRepository.CreateOrUpdateBasketAsync(basket);

            if (savedBasket is null)
                return Result<CustomerBasketDto>.fail(Error.Validation("Could not save the basket."));

            return Result<CustomerBasketDto>.ok(_mapper.Map<CustomerBasketDto>(savedBasket));
        }

        public async Task<Result> DeleteBasketAsync(string id)
        {
            var deleted = await _basketRepository.DeleteBasketAsync(id);
            return deleted
                ? Result.ok()
                : Result.fail(Error.NotFound($"Basket with id {id} was not found."));
        }
    }
}
