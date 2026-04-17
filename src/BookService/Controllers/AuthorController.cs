using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BS.WebAPI.Controllers
{
    /// <summary>
    /// Operaciones CRUD sobre autores. Requiere JWT.
    /// </summary>
    [Route("api/authors")]
    [ApiController]
    [Authorize]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<PagedResponseDto<AuthorResponseDto>> GetAll([FromQuery] QueryAuthorDto query)
        {
            return await _authorService.GetAll(query);
        }

        [HttpGet("{id:guid}")]
        public async Task<ResponseDto<AuthorResponseDto>> GetById(Guid id)
        {
            return await _authorService.GetById(id);
        }

        [HttpPost]
        public async Task<ResponseDto<AuthorResponseDto>> Create([FromBody] CreateAuthorDto dto)
        {
            return await _authorService.Create(dto);
        }

        [HttpPatch("{id:guid}")]
        public async Task<BaseResponseDto> Update(Guid id, [FromBody] UpdateAuthorDto dto)
        {
            return await _authorService.Update(id, dto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<BaseResponseDto> Delete(Guid id)
        {
            return await _authorService.Delete(id);
        }
    }
}
