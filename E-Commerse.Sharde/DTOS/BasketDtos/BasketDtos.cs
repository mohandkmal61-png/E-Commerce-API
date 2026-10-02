namespace E_Commerse.Sharde.DTOS.BasketDtos
{
    public class CustomerBasketDto
    {
        public string Id { get; set; } = null!;

        public List<BasketItemDto> Items { get; set; } = [];
    }

    public class BasketItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string PictureUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
