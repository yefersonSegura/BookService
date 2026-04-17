using BS.Application.DTOs;
using BS.Domain.Entity;

namespace BS.Application.Common
{
    internal static class BookResponseMapper
    {
        public static BookResponseDto FromEntity(Book book)
        {
            return new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PublicationYear = book.PublicationYear,
                CoverUrl = book.CoverUrl,
                AuthorName = book.Author?.Name
            };
        }
    }
}
