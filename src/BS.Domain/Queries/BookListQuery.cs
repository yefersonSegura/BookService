namespace BS.Domain.Queries
{
    public class BookListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string Title { get; set; } = string.Empty;

        public string AutorName { get; set; } = string.Empty;
    }
}
