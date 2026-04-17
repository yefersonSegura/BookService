using BS.Application.DTOs;
using BS.Domain.Entity;

namespace BS.Application.Common
{
    internal static class AuthorResponseMapper
    {
        public static AuthorResponseDto FromEntity(Author author)
        {
            return new AuthorResponseDto
            {
                Id = author.Id,
                Name = author.Name ?? string.Empty,
                BirthDate = author.BirthDate
            };
        }
    }
}
