using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace E_Commerse.Sharde.DTOS.ProdactDtos
{
    public class ProdactDtos
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        [JsonPropertyName("PictureUrl")]
        public string PuctureUrl { get; set; } = null!;

        public decimal Price { get; set; }
        public string ProductBrand { get; set; } = null!;   
        public string ProductType { get; set; } = null!;    


    }
}
