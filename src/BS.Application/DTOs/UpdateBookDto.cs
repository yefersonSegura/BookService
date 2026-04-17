using System.ComponentModel.DataAnnotations;

namespace BS.Application.DTOs
{
    public class UpdateBookDto
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 200 caracteres.")]
        public string? Title { get; set; }

        [StringLength(20, MinimumLength = 10, ErrorMessage = "El ISBN debe tener entre 10 y 20 caracteres.")]
        public string? Isbn { get; set; }

        [Range(1400, 2100, ErrorMessage = "El año de publicación debe estar entre 1400 y 2100.")]
        public int? PublicationYear { get; set; }
    }
}
