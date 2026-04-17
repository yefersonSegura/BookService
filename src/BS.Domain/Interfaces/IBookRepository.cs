using BS.Domain.Entity;
using BS.Domain.Queries;

namespace BS.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<(List<Book> Items, int TotalCount)> GetAll(BookListQuery query);
        Task<Book?> GetById(Guid id);
        Task Add(Book book);
        Task Update(Book book);
        Task<bool> Delete(Guid id);
    }
}
