using BS.Application.Common.DTOs;
using BS.Application.DTOs;

namespace BS.Application.Interfaces
{
    public interface IBookService
    {
        Task<PagedResponseDto<BookResponseDto>> GetAll(QueryBookDto query);
        Task<ResponseDto<BookResponseDto>> GetById(Guid id);
        Task<ResponseDto<BookResponseDto>> Create(CreateBookDto dto);
        Task<BaseResponseDto> Update(Guid id, UpdateBookDto dto);
        Task<BaseResponseDto> Delete(Guid id);
        Task<BaseResponseDto> ValidateIsbn(string isbn);
        Task<ResponseDto<List<BookResponseDto>>> CreateMassive(List<CreateBookMassiveItemDto> books);

        Task<ResponseDto<List<BookResponseDto>>> UploadFromCsv(
            Stream? csvStream,
            string? fileName,
            long fileLength,
            CancellationToken cancellationToken = default);

        /// <summary>Plantilla UTF-8 (con BOM) para importación CSV de libros.</summary>
        (byte[] Utf8Content, string FileDownloadName) GetCsvUploadTemplate();
    }
}
