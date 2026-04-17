using System.ComponentModel.DataAnnotations;
using System.IO;
using BS.Application.Common;
using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using BS.Domain.Enitity;
using BS.Domain.Interfaces;
using BS.Domain.Queries;

namespace BS.Application.Services
{
    internal sealed class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IIsbnSoapValidator _isbnValidator;
        private readonly IOpenLibraryCoverClient _coverClient;

        public BookService(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IIsbnSoapValidator isbnValidator,
            IOpenLibraryCoverClient coverClient)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _isbnValidator = isbnValidator;
            _coverClient = coverClient;
        }
        public async Task<ResponseDto<BookResponseDto>> Create(CreateBookDto dto)
        {
            var response = new ResponseDto<BookResponseDto>();
            try
            {
                if (!await _authorRepository.Exists(dto.AuthorId).ConfigureAwait(false))
                {
                    ServiceResponseBuilder.ApplyBookFailure(response, 400, "El autor no existe.", "El autor no existe.");
                    return response;
                }

                return await CreateBookCore(dto.AuthorId, dto.Isbn, dto.Title, dto.PublicationYear).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<ResponseDto<List<BookResponseDto>>> CreateMassive(List<CreateBookMassiveItemDto> books)
        {
            var response = new ResponseDto<List<BookResponseDto>>();
            try
            {
                if (books == null || books.Count == 0)
                {
                    ServiceResponseBuilder.ApplyDtoFailure(
                        response,
                        400,
                        "Debe enviar al menos un libro.",
                        "La lista está vacía.");
                    return response;
                }

                var list = new List<BookResponseDto>();
                for (var index = 0; index < books.Count; index++)
                {
                    var dto = books[index];
                    var itemPrefix = $"Ítem {index + 1}: libro{dto.Title}";

                    var authorResult = await TryGetOrCreateAuthorByName(dto.AuthorName).ConfigureAwait(false);
                    if (!authorResult.Ok)
                    {
                        response.IsSuccessful = false;
                        response.Status = 400;
                        response.Result = 0;
                        response.Message = "Error en la carga masiva.";
                        response.Data = null;
                        response.Errors.Clear();
                        response.Errors.Add(itemPrefix + authorResult.Error);
                        return response;
                    }

                    var one = await CreateBookCore(authorResult.AuthorId, dto.Isbn, dto.Title, dto.PublicationYear).ConfigureAwait(false);
                    if (!one.IsSuccessful || one.Data == null)
                    {
                        response.IsSuccessful = false;
                        response.Status = one.Status;
                        response.Result = 0;
                        response.Message = "Error en la carga masiva.";
                        response.Data = null;
                        response.Errors.Clear();
                        var detail = one.Errors.Count > 0 ? one.Errors[0] : one.Message ?? "Error desconocido.";
                        response.Errors.Add(itemPrefix + detail);
                        return response;
                    }

                    list.Add(one.Data);
                }

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = list.Count;
                response.Message = "Libros creados.";
                response.Data = list;
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                response.Data = null;
                return response;
            }
        }

        public async Task<ResponseDto<List<BookResponseDto>>> UploadFromCsv(
            Stream? csvStream,
            string? fileName,
            long fileLength,
            CancellationToken cancellationToken = default)
        {
            var response = new ResponseDto<List<BookResponseDto>>();
            if (csvStream == null)
            {
                ServiceResponseBuilder.ApplyCsvUploadFailure(
                    response,
                    "Debe enviar un archivo CSV en el formulario (campo file).",
                    "Archivo no enviado.");
                return response;
            }

            await using (csvStream)
            {
                try
                {
                    if (fileLength <= 0)
                    {
                        ServiceResponseBuilder.ApplyCsvUploadFailure(
                            response,
                            "Debe enviar un archivo CSV en el formulario (campo file).",
                            "Archivo vacío o no enviado.");
                        return response;
                    }

                    var extension = Path.GetExtension(fileName ?? string.Empty);
                    if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        ServiceResponseBuilder.ApplyCsvUploadFailure(
                            response,
                            "Solo se aceptan archivos con extensión .csv.",
                            "Extensión de archivo no válida.");
                        return response;
                    }

                    var (ok, items, parseError) = await CsvBookImportParser.Parse(csvStream, cancellationToken).ConfigureAwait(false);
                    if (!ok || items == null)
                    {
                        response.IsSuccessful = false;
                        response.Status = 400;
                        response.Result = 0;
                        response.Message = parseError ?? "No se pudo leer el CSV.";
                        response.Data = null;
                        response.Errors.Clear();
                        if (!string.IsNullOrEmpty(parseError))
                        {
                            response.Errors.Add(parseError);
                        }

                        return response;
                    }

                    for (var i = 0; i < items.Count; i++)
                    {
                        var row = items[i];
                        var validationContext = new ValidationContext(row);
                        var validationResults = new List<ValidationResult>();
                        if (!Validator.TryValidateObject(row, validationContext, validationResults, validateAllProperties: true))
                        {
                            response.IsSuccessful = false;
                            response.Status = 400;
                            response.Result = 0;
                            response.Message = "Datos inválidos en el CSV.";
                            response.Data = null;
                            response.Errors.Clear();
                            var messages = string.Join(
                                "; ",
                                validationResults.Select(static r => r.ErrorMessage).Where(static m => !string.IsNullOrEmpty(m)));
                            response.Errors.Add($"Fila de datos {i + 2}: {messages}");
                            return response;
                        }
                    }

                    return await CreateMassive(items).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                    response.Data = null;
                    return response;
                }
            }
        }

        public (byte[] Utf8Content, string FileDownloadName) GetCsvUploadTemplate()
        {
            return (CsvBookImportParser.GetTemplateUtf8WithBom(), CsvBookImportParser.TemplateFileName);
        }

        private async Task<ResponseDto<BookResponseDto>> CreateBookCore(
            Guid authorId,
            string isbnInput,
            string titleRaw,
            int publicationYear)
        {
            var response = new ResponseDto<BookResponseDto>();
            var trimmedIsbn = isbnInput.Trim();
            if (!await _isbnValidator.IsValid(trimmedIsbn).ConfigureAwait(false))
            {
                ServiceResponseBuilder.ApplyBookFailure(
                    response,
                    400,
                    "El ISBN no es válido según el servicio de validación.",
                    "El ISBN no es válido según el servicio de validación.");
                return response;
            }

            var normalizedIsbn = IsbnText.Clean(trimmedIsbn);
            var coverUrl = await _coverClient.GetThumbnailUrl(normalizedIsbn).ConfigureAwait(false);
            var normalizedTitle = TextNormalizer.NormalizeForPersistence(titleRaw);

            var book = new Book
            {
                Isbn = normalizedIsbn,
                Title = normalizedTitle,
                PublicationYear = publicationYear,
                PageNumber = 0,
                CoverUrl = coverUrl,
                AuthorId = authorId
            };

            await _bookRepository.Add(book).ConfigureAwait(false);
            var created = await _bookRepository.GetById(book.Id).ConfigureAwait(false);

            response.IsSuccessful = true;
            response.Status = 200;
            response.Result = 1;
            response.Message = "Libro creado.";
            response.Data = BookResponseMapper.FromEntity(created!);
            response.Errors.Clear();
            return response;
        }

        private async Task<(bool Ok, Guid AuthorId, string Error)> TryGetOrCreateAuthorByName(string authorName)
        {
            var normalized = TextNormalizer.NormalizeForPersistence(authorName);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return (false, default, "El nombre del autor no es válido después de normalizar.");
            }

            var existing = await _authorRepository.GetByNormalizedName(normalized).ConfigureAwait(false);
            if (existing != null)
            {
                return (true, existing.Id, string.Empty);
            }

            var author = new Author
            {
                Name = normalized,
                BirthDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            await _authorRepository.Add(author).ConfigureAwait(false);
            return (true, author.Id, string.Empty);
        }

        public async Task<BaseResponseDto> Delete(Guid id)
        {
            var response = new BaseResponseDto();
            try
            {
                var ok = await _bookRepository.Delete(id).ConfigureAwait(false);
                if (!ok)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Libro no encontrado.";
                    response.Errors.Clear();
                    response.Errors.Add("Libro no encontrado.");
                    return response;
                }

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = "Libro eliminado.";
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<PagedResponseDto<BookResponseDto>> GetAll(QueryBookDto query)
        {
            var response = new PagedResponseDto<BookResponseDto>
            {
                Data = new List<BookResponseDto>(),
                Total = 0
            };
            try
            {
                var listQuery = new BookListQuery
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    Title = query.Title,
                    AutorName = query.AutorName
                };
                var (items, total) = await _bookRepository.GetAll(listQuery).ConfigureAwait(false);

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = items.Count;
                response.Message = null;
                response.Total = total;
                response.Data = items.Select(BookResponseMapper.FromEntity).ToList();
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<ResponseDto<BookResponseDto>> GetById(Guid id)
        {
            var response = new ResponseDto<BookResponseDto>();
            try
            {
                var book = await _bookRepository.GetById(id).ConfigureAwait(false);
                if (book == null)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Libro no encontrado.";
                    response.Data = null;
                    response.Errors.Clear();
                    response.Errors.Add("Libro no encontrado.");
                    return response;
                }

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = null;
                response.Data = BookResponseMapper.FromEntity(book);
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<BaseResponseDto> Update(Guid id, UpdateBookDto dto)
        {
            var response = new BaseResponseDto();
            try
            {
                var book = await _bookRepository.GetById(id).ConfigureAwait(false);
                if (book == null)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Libro no encontrado.";
                    response.Errors.Clear();
                    response.Errors.Add("Libro no encontrado.");
                    return response;
                }

                if (dto.Isbn != null)
                {
                    var trimmed = dto.Isbn.Trim();
                    if (!await _isbnValidator.IsValid(trimmed).ConfigureAwait(false))
                    {
                        response.IsSuccessful = false;
                        response.Status = 400;
                        response.Result = 0;
                        response.Message = "El ISBN no es válido según el servicio de validación.";
                        response.Errors.Clear();
                        response.Errors.Add("El ISBN no es válido según el servicio de validación.");
                        return response;
                    }

                    book.Isbn = IsbnText.Clean(trimmed);
                    book.CoverUrl = await _coverClient.GetThumbnailUrl(book.Isbn).ConfigureAwait(false);
                }

                if (dto.Title != null)
                {
                    book.Title = TextNormalizer.NormalizeForPersistence(dto.Title);
                }

                if (dto.PublicationYear.HasValue)
                {
                    book.PublicationYear = dto.PublicationYear.Value;
                }

                await _bookRepository.Update(book).ConfigureAwait(false);

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = "Libro actualizado.";
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<BaseResponseDto> ValidateIsbn(string isbn)
        {
            var response = new BaseResponseDto();
            try
            {
                var ok = await _isbnValidator.IsValid(isbn).ConfigureAwait(false);
                response.IsSuccessful = ok;
                response.Status = ok ? 200 : 400;
                response.Result = ok ? 1 : 0;
                response.Message = ok ? "ISBN válido." : "ISBN no válido.";
                response.Errors.Clear();
                if (!ok)
                {
                    response.Errors.Add("ISBN no válido.");
                }

                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }
    }
}
