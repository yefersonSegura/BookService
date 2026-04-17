using BS.Domain.Entity;
using BS.Domain.Queries;

namespace BS.Domain.Interfaces
{
    public interface IAuthorRepository
    {
        Task<bool> Exists(Guid id, CancellationToken cancellationToken = default);

        Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default);

        Task<Author?> GetByNormalizedName(string normalizedName, CancellationToken cancellationToken = default);

        Task<(List<Author> Items, int TotalCount)> GetAll(AuthorListQuery query, CancellationToken cancellationToken = default);

        Task Add(Author author, CancellationToken cancellationToken = default);

        Task Update(Author author, CancellationToken cancellationToken = default);

        Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
    }
}
