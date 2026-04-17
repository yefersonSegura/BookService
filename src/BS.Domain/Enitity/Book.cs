using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BS.Domain.Enitity
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? CoverUrl { get; set; }
        public int PublicationYear { get; set; }
        public int PageNumber { get; set; }
        public Guid AuthorId { get; set; }
        public Author? Author { get; set; }
    }
}
