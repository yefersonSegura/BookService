using System;
using System.Collections.Generic;

namespace BS.Domain.Entity;

public class Author
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public DateTime BirthDate { get; set; }
    public List<Book> Books { get; set; } = new();
}
