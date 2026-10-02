using AutoMapper;
using ECommerce.Domin.Entitie.ProductModule;
using E_Commerse.Sharde.DTOS.ProdactDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Services.MapingProfile
{
    public  class ProducteProfile : Profile
    {
        public ProducteProfile()
        {
            
            CreateMap <ProductBrand, BrandDtos>();
            CreateMap<CreateProductDto, Product>();
             CreateMap<Product , ProdactDtos>()
                  .ForMember(dest => dest.ProductBrand, opt => opt.MapFrom(src => src.ProductBrands.Name)) 
                    .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductTypes.Name));  
                CreateMap<ProductType, TypeDtos>(); 
        }
    }
}
