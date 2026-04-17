using System.ComponentModel.DataAnnotations;

namespace BS.Application.DTOs
{
    /// <summary>
    /// Libro en carga masiva (POST massive o CSV): <c>authorName</c>. El alta individual (POST /books) usa <c>CreateBookDto</c> con <c>authorId</c>.
    /// </summary>
    public class CreateBookMassiveItemDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 200 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ISBN es obligatorio.")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El ISBN debe tener entre 10 y 20 caracteres.")]
        public string Isbn { get; set; } = string.Empty;

        [Range(1400, 2100, ErrorMessage = "El año de publicación debe estar entre 1400 y 2100.")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "El nombre del autor es obligatorio.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre del autor debe tener entre 2 y 150 caracteres.")]
        public string AuthorName { get; set; } = string.Empty;
    }
}
