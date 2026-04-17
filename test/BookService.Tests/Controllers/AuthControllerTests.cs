using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using BS.WebAPI.Controllers;
using BS.WebAPI.DTOs;
using Moq;
using Xunit;

namespace BookService.Tests.Controllers;

/// <summary>
/// Pruebas del controlador de autenticación con <see cref="ILoginService"/> simulado.
/// </summary>
public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_delegue_en_ILoginService_y_devuelve_su_respuesta()
    {
        var expected = new ResponseDto<LoginTokenDataDto>
        {
            IsSuccessful = true,
            Status = 200,
            Result = 1,
            Message = "Autenticación correcta.",
            Data = new LoginTokenDataDto
            {
                AccessToken = "token_de_prueba",
                TokenType = "Bearer",
                ExpiresInSeconds = 3600,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            }
        };

        var mockLogin = new Mock<ILoginService>();
        mockLogin
            .Setup(s => s.LoginAsync("admin", "BookAdmin01!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new AuthController(mockLogin.Object);

        var actual = await controller.Login(
            new LoginRequest { Username = "admin", Password = "BookAdmin01!" },
            CancellationToken.None);

        Assert.Same(expected, actual);
        mockLogin.Verify(
            s => s.LoginAsync("admin", "BookAdmin01!", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
