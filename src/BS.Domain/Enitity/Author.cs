using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BS.Domain.Enitity
{
    public class Author
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string? Name { get; set; }
        public DateTime BirthDate { get; set; }
        public List<Book> Books { get; set; } = new();
    }
}
