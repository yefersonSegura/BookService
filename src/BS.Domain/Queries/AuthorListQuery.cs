namespace BS.Domain.Queries
{
    public class AuthorListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string AuthorName { get; set; } = string.Empty;
    }
}
