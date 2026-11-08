namespace E_Commerse.Sharde.DTOS.BasketDtos
{
    public class CustomerBasketDto
    {
        public string Id { get; set; } = null!;

        public long Version { get; set; }

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

    public class SaveBasketRequestDto
    {
        public string Id { get; set; } = null!;
        public long ExpectedVersion { get; set; }
        public List<BasketItemDto> Items { get; set; } = [];
    }

    public class MergeBasketRequestDto
    {
        public string GuestBasketId { get; set; } = null!;
        public string GuestSecret { get; set; } = null!;
    }
}
