namespace E_Commerse.Sharde.DTOS.Pagination
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;

        public int PageIndex { get; set; } = 0;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
