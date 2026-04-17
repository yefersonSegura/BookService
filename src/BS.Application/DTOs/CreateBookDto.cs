using System.ComponentModel.DataAnnotations;

namespace BS.Application.DTOs
{
    public class CreateBookDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 200 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ISBN es obligatorio.")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El ISBN debe tener entre 10 y 20 caracteres.")]
        public string Isbn { get; set; } = string.Empty;

        [Range(1400, 2100, ErrorMessage = "El año de publicación debe estar entre 1400 y 2100.")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "El autor es obligatorio.")]
        public Guid AuthorId { get; set; }
    }
}
