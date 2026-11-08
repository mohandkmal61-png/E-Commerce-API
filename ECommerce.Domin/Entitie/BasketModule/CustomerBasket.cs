namespace ECommerce.Domin.Entitie.BasketModule
{
    public class CustomerBasket
    {
        public string Id { get; set; } = null!;

        public string? OwnerUserId { get; set; }

        public string? GuestCredentialHash { get; set; }

        public long Version { get; set; }

        public List<BasketItem> Items { get; set; } = [];
    }
}
