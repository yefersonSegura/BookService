using System.ComponentModel.DataAnnotations;

namespace BS.Application.Common.DTOs;
public class PagedRequestDto
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;

    /// <summary>Página (1-based). Si no se envía, se usa <see cref="DefaultPage"/>.</summary>
    [Range(1, 10_000, ErrorMessage = "La página debe estar entre 1 y 10000.")]
    public int? Page { get; set; }

    /// <summary>Tamaño de página. Si no se envía, se usa <see cref="DefaultPageSize"/> (máx. <see cref="MaxPageSize"/>).</summary>
    [Range(1, MaxPageSize, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int? PageSize { get; set; }

    /// <summary>Valores efectivos para repositorio (defaults si vienen ausentes o inválidos).</summary>
    public (int Page, int PageSize) GetNormalizedPaging()
    {
        var page = Page is >= 1 ? Page.Value : DefaultPage;
        var size = PageSize is >= 1 && PageSize <= MaxPageSize
            ? PageSize.Value
            : (PageSize is > MaxPageSize ? MaxPageSize : DefaultPageSize);
        return (page, size);
    }
}
