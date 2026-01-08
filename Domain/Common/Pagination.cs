
namespace Domain.Common
{
    public record PaginationParameters
    {
        private const int MaxPageSize = 200;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = (value <= 0) ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = (value <= 0) ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }
    }

    public record PagedList<T>
    {
        public IReadOnlyList<T> Items { get; init; }
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public long TotalCount { get; init; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(IReadOnlyList<T> items, long totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            Items = items;
        }

        public static PagedList<T> Create(IEnumerable<T> source, long count, int pageNumber, int pageSize)
        {
            return new PagedList<T>(source.ToList(), count, pageNumber, pageSize);
        }
    }
}
