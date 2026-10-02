namespace E_Commerse.Sharde.Constants
{
    public static class BasketConstants
    {
        // Basket is removed automatically if nobody uses it for this period (sliding expiration).
        public static readonly TimeSpan TimeToLive = TimeSpan.FromDays(7);
    }
}
