using System;
using System.Collections.Generic;

namespace BS.Domain.Entity;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public int PublicationYear { get; set; }
    /// <summary>Número de páginas del libro.</summary>
    public int PageCount { get; set; }
    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
}
