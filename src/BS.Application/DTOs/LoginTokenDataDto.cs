namespace BS.Application.DTOs;

/// <summary>
/// Datos del JWT en la propiedad <c>data</c> de la respuesta cuando el login es correcto.
/// </summary>
public sealed class LoginTokenDataDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
