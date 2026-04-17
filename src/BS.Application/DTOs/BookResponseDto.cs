using System;
using System.Collections.Generic;
using System.Text;

namespace BS.Application.DTOs
{
    public class BookResponseDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Isbn { get; set; }
        public int PublicationYear { get; set; }
        public string? CoverUrl { get; set; }
        public string? AuthorName { get; set; }
    }
}
