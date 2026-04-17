using System.ComponentModel.DataAnnotations;
using BS.Application.Common.DTOs;

namespace BS.Application.DTOs
{
    public class QueryBookDto : PagedRequestDto
    {
        [MaxLength(200, ErrorMessage = "El filtro de título admite como máximo 200 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "El filtro de autor admite como máximo 150 caracteres.")]
        public string AutorName { get; set; } = string.Empty;
    }
}
