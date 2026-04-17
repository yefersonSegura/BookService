using BS.Application.Common;
using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using BS.Domain.Enitity;
using BS.Domain.Interfaces;
using BS.Domain.Queries;

namespace BS.Application.Services
{
    internal sealed class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<ResponseDto<AuthorResponseDto>> Create(CreateAuthorDto dto)
        {
            var response = new ResponseDto<AuthorResponseDto>();
            try
            {
                var normalizedName = TextNormalizer.NormalizeForPersistence(dto.Name);
                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    ServiceResponseBuilder.ApplyDtoFailure(
                        response,
                        400,
                        "El nombre del autor no es válido después de normalizar.",
                        "El nombre del autor no es válido después de normalizar.");
                    return response;
                }

                var duplicate = await _authorRepository.GetByNormalizedName(normalizedName).ConfigureAwait(false);
                if (duplicate != null)
                {
                    ServiceResponseBuilder.ApplyDtoFailure(
                        response,
                        409,
                        "Ya existe un autor con el mismo nombre (normalizado).",
                        "Nombre de autor duplicado.");
                    return response;
                }

                var birthDate = dto.BirthDate?.Date
                    ?? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                var author = new Author
                {
                    Name = normalizedName,
                    BirthDate = birthDate
                };

                await _authorRepository.Add(author).ConfigureAwait(false);
                var created = await _authorRepository.GetById(author.Id).ConfigureAwait(false);

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = "Autor creado.";
                response.Data = AuthorResponseMapper.FromEntity(created!);
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<BaseResponseDto> Delete(Guid id)
        {
            var response = new BaseResponseDto();
            try
            {
                var ok = await _authorRepository.Delete(id).ConfigureAwait(false);
                if (!ok)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Autor no encontrado.";
                    response.Errors.Clear();
                    response.Errors.Add("Autor no encontrado.");
                    return response;
                }

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = "Autor eliminado.";
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<PagedResponseDto<AuthorResponseDto>> GetAll(QueryAuthorDto query)
        {
            var response = new PagedResponseDto<AuthorResponseDto>
            {
                Data = new List<AuthorResponseDto>(),
                Total = 0
            };
            try
            {
                var filterName = string.IsNullOrWhiteSpace(query.AuthorName)
                    ? string.Empty
                    : TextNormalizer.NormalizeForPersistence(query.AuthorName);
                var listQuery = new AuthorListQuery
                {
                    Page = query.Page,
                    PageSize = query.PageSize,
                    AuthorName = filterName
                };
                var (items, total) = await _authorRepository.GetAll(listQuery).ConfigureAwait(false);

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = items.Count;
                response.Message = null;
                response.Total = total;
                response.Data = items.Select(AuthorResponseMapper.FromEntity).ToList();
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                response.Total = 0;
                response.Data = new List<AuthorResponseDto>();
                return response;
            }
        }

        public async Task<ResponseDto<AuthorResponseDto>> GetById(Guid id)
        {
            var response = new ResponseDto<AuthorResponseDto>();
            try
            {
                var author = await _authorRepository.GetById(id).ConfigureAwait(false);
                if (author == null)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Autor no encontrado.";
                    response.Data = null;
                    response.Errors.Clear();
                    response.Errors.Add("Autor no encontrado.");
                    return response;
                }

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = null;
                response.Data = AuthorResponseMapper.FromEntity(author);
                response.Errors.Clear();
                return response;
            }
            catch (Exception ex)
            {
                ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
                return response;
            }
        }

        public async Task<BaseResponseDto> Update(Guid id, UpdateAuthorDto dto)
        {
            var response = new BaseResponseDto();
            try
            {
                var author = await _authorRepository.GetById(id).ConfigureAwait(false);
                if (author == null)
                {
                    response.IsSuccessful = false;
                    response.Status = 404;
                    response.Result = 0;
                    response.Message = "Autor no encontrado.";
                    response.Errors.Clear();
                    response.Errors.Add("Autor no encontrado.");
                    return response;
                }

                var normalizedName = TextNormalizer.NormalizeForPersistence(dto.Name);
                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    response.IsSuccessful = false;
                    response.Status = 400;
                    response.Result = 0;
                    response.Message = "El nombre del autor no es válido después de normalizar.";
                    response.Errors.Clear();
                    response.Errors.Add("El nombre del autor no es válido después de normalizar.");
                    return response;
                }

                var other = await _authorRepository.GetByNormalizedName(normalizedName).ConfigureAwait(false);
                if (other != null && other.Id != id)
                {
                    response.IsSuccessful = false;
                    response.Status = 409;
                    response.Result = 0;
                    response.Message = "Ya existe otro autor con el mismo nombre (normalizado).";
                    response.Errors.Clear();
                    response.Errors.Add("Nombre de autor duplicado.");
                    return response;
                }

                author.Name = normalizedName;
                if (dto.BirthDate.HasValue)
                {
                    author.BirthDate = dto.BirthDate.Value.Date;
                }

                await _authorRepository.Update(author).ConfigureAwait(false);

                response.IsSuccessful = true;
                response.Status = 200;
                response.Result = 1;
                response.Message = "Autor actualizado.";
                response.Errors.Clear();
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
