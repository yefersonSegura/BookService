using System.ComponentModel.DataAnnotations;

namespace BS.Application.Common.DTOs
{
    public class PagedRequestDto
    {
        protected PagedRequestDto()
        {
            Page = 1;
            PageSize = 50;
        }

        [Range(1, 10_000, ErrorMessage = "La página debe estar entre 1 y 10000.")]
        public int Page { get; set; }

        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
        public int PageSize { get; set; }
    }
}
