using E_Commerse.Sharde.CommenResult;
using E_Commerse.Sharde.DTOS.BasketDtos;

namespace ECommerce.ServicesAbstraction
{
    public interface IBasketServise
    {
        Task<Result<CustomerBasketDto>> GetBasketByIdAsync(string id);
        Task<Result<CustomerBasketDto>> CreateOrUpdateBasketAsync(CustomerBasketDto basketDto);
        Task<Result> DeleteBasketAsync(string id);
    }
}
