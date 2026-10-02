namespace E_Commerse.Sharde.DTOS.ProdactDtos
{
    public class PaginatedResult<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(Count / (double)PageSize) : 0;
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex + 1 < TotalPages;
        public IEnumerable<T> Data { get; set; } = [];

        public static PaginatedResult<T> Create(int pageIndex, int pageSize, int count, IEnumerable<T> data)
            => new()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Count = count,
                Data = data
            };
    }
}
