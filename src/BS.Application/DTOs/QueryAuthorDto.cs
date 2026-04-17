using System.ComponentModel.DataAnnotations;
using BS.Application.Common.DTOs;

namespace BS.Application.DTOs
{
    public class QueryAuthorDto : PagedRequestDto
    {
        [MaxLength(150, ErrorMessage = "El filtro de nombre admite como máximo 150 caracteres.")]
        public string AuthorName { get; set; } = string.Empty;
    }
}
