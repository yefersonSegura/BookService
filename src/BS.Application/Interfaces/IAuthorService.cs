using BS.Application.Common.DTOs;
using BS.Application.DTOs;

namespace BS.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<PagedResponseDto<AuthorResponseDto>> GetAll(QueryAuthorDto query);

        Task<ResponseDto<AuthorResponseDto>> GetById(Guid id);

        Task<ResponseDto<AuthorResponseDto>> Create(CreateAuthorDto dto);

        Task<BaseResponseDto> Update(Guid id, UpdateAuthorDto dto);

        Task<BaseResponseDto> Delete(Guid id);
    }
}
