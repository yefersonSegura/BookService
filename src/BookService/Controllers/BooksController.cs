using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BS.WebAPI.Controllers
{
    /// <summary>
    /// Operaciones CRUD y utilidades (validación ISBN, carga CSV) sobre libros. Requiere JWT.
    /// </summary>
    [Route("api/books")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<PagedResponseDto<BookResponseDto>> GetAll([FromQuery] QueryBookDto query)
        {
            return await _bookService.GetAll(query);
        }

        [HttpGet("{id:guid}")]
        public async Task<ResponseDto<BookResponseDto>> GetById(Guid id)
        {
            return await _bookService.GetById(id);
        }

        [HttpPost]
        public async Task<ResponseDto<BookResponseDto>> Create([FromBody] CreateBookDto dto)
        {
            return await _bookService.Create(dto);
        }

        [HttpPatch("{id:guid}")]
        public async Task<BaseResponseDto> Update(Guid id, [FromBody] UpdateBookDto dto)
        {
            return await _bookService.Update(id, dto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<BaseResponseDto> Delete(Guid id)
        {
            return await _bookService.Delete(id);
        }

        [HttpGet("validation/{isbn}")]
        public async Task<BaseResponseDto> ValidateIsbn(string isbn)
        {
            return await _bookService.ValidateIsbn(isbn);
        }

        [HttpPost("massive")]
        public async Task<ResponseDto<List<BookResponseDto>>> CreateMassive([FromBody] List<CreateBookMassiveItemDto> books)
        {
            return await _bookService.CreateMassive(books);
        }

        [HttpGet("upload/template")]
        public IActionResult DownloadUploadTemplate()
        {
            var (bytes, fileName) = _bookService.GetCsvUploadTemplate();
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpPost("upload")]
        [RequestSizeLimit(32 * 1024 * 1024)]
        public Task<ResponseDto<List<BookResponseDto>>> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            return _bookService.UploadFromCsv(file?.OpenReadStream(), file?.FileName, file?.Length ?? 0, cancellationToken);
        }
    }
}
