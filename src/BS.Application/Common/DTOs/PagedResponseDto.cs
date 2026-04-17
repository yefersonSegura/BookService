using System;
using System.Collections.Generic;
using System.Text;

namespace BS.Application.Common.DTOs
{
    public class PagedResponseDto<T> : BaseResponseDto
    {
        /// <summary>Total de registros que cumplen el filtro (todas las páginas).</summary>
        public long Total { get; set; }

        /// <summary>Página devuelta (1-based), tras normalización.</summary>
        public int Page { get; set; }

        /// <summary>Tamaño de página aplicado.</summary>
        public int PageSize { get; set; }

        public List<T>? Data { get; set; }
    }
}
