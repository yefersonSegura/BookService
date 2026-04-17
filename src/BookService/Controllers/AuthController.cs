using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using BS.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BS.WebAPI.Controllers;

/// <summary>
/// Autenticación (emisión de JWT). Los endpoints de login son anónimos.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(ILoginService loginService) : ControllerBase
{
    /// <summary>
    /// Valida credenciales y devuelve un token JWT (vigencia 1 hora) en <c>data</c> si <c>isSuccessful</c> es verdadero.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResponseDto<LoginTokenDataDto>), StatusCodes.Status200OK)]
    public async Task<ResponseDto<LoginTokenDataDto>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return await loginService.LoginAsync(request.Username, request.Password, cancellationToken).ConfigureAwait(false);
    }
}
