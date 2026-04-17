using BS.Application.Common.DTOs;
using BS.Application.DTOs;

namespace BS.Application.Interfaces;

public interface ILoginService
{
    Task<ResponseDto<LoginTokenDataDto>> LoginAsync(
        string? username,
        string? password,
        CancellationToken cancellationToken = default);
}
