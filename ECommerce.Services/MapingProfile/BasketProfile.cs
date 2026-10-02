using AutoMapper;
using ECommerce.Domin.Entitie.BasketModule;
using E_Commerse.Sharde.DTOS.BasketDtos;

namespace ECommerce.Services.MapingProfile
{
    public class BasketProfile : Profile
    {
        public BasketProfile()
        {
            CreateMap<CustomerBasket, CustomerBasketDto>().ReverseMap();
            CreateMap<BasketItem, BasketItemDto>().ReverseMap();
        }
    }
}
