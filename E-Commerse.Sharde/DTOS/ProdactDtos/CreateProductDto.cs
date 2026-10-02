namespace E_Commerse.Sharde.DTOS.ProdactDtos
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PuctureUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int BrandId { get; set; }
        public int TypeId { get; set; }
    }
}
