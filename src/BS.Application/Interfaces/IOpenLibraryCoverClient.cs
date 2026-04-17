namespace BS.Application.Interfaces
{
    public interface IOpenLibraryCoverClient
    {
        Task<string?> GetThumbnailUrl(string isbn, CancellationToken cancellationToken = default);
    }
}
