using BS.Domain.Enitity;
using BS.Domain.Interfaces;
using BS.Domain.Queries;
using BS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace _04.BS.Infrastructure.Repository
{
    internal sealed class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Add(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Delete(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return false;
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Book> Items, int TotalCount)> GetAll(BookListQuery query)
        {
            var q = _context.Books.Include(b => b.Author).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                q = q.Where(b => b.Title.Contains(query.Title));
            }

            if (!string.IsNullOrWhiteSpace(query.AutorName))
            {
                q = q.Where(b => b.Author != null && b.Author.Name != null && b.Author.Name.Contains(query.AutorName));
            }

            var total = await q.CountAsync();
            var page = Math.Max(1, query.Page);
            var size = Math.Clamp(query.PageSize, 1, 100);
            var items = await q
                .OrderBy(b => b.Title)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            return (items, total);
        }

        public async Task<Book?> GetById(Guid id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task Update(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }
    }
}
