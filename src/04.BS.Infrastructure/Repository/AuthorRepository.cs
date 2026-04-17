using BS.Domain.Enitity;
using BS.Domain.Interfaces;
using BS.Domain.Queries;
using BS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace _04.BS.Infrastructure.Repository
{
    internal sealed class AuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _context;

        public AuthorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Add(Author author, CancellationToken cancellationToken = default)
        {
            await _context.Authors.AddAsync(author, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var author = await _context.Authors.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (author == null)
            {
                return false;
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Authors.AnyAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<(List<Author> Items, int TotalCount)> GetAll(
            AuthorListQuery query,
            CancellationToken cancellationToken = default)
        {
            var q = _context.Authors.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.AuthorName))
            {
                q = q.Where(a => a.Name != null && a.Name.Contains(query.AuthorName));
            }

            var total = await q.CountAsync(cancellationToken).ConfigureAwait(false);
            var page = Math.Max(1, query.Page);
            var size = Math.Clamp(query.PageSize, 1, 100);
            var items = await q
                .OrderBy(a => a.Name)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return (items, total);
        }

        public Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public Task<Author?> GetByNormalizedName(string normalizedName, CancellationToken cancellationToken = default)
        {
            return _context.Authors.FirstOrDefaultAsync(a => a.Name == normalizedName, cancellationToken);
        }

        public async Task Update(Author author, CancellationToken cancellationToken = default)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
