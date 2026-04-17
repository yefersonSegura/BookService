using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BS.Application.Common;
using BS.Application.Common.DTOs;
using BS.Application.DTOs;
using BS.Application.Interfaces;
using BS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BS.WebAPI.Services;

internal sealed class LoginService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration) : ILoginService
{
    public async Task<ResponseDto<LoginTokenDataDto>> LoginAsync(
        string? username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseDto<LoginTokenDataDto>();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ServiceResponseBuilder.ApplyDtoFailure(
                response,
                400,
                "Usuario y contraseña son obligatorios.",
                "Usuario y contraseña son obligatorios.");
            return response;
        }

        var user = await userManager.FindByNameAsync(username.Trim()).ConfigureAwait(false);
        if (user == null)
        {
            ServiceResponseBuilder.ApplyDtoFailure(
                response,
                401,
                "Credenciales inválidas.",
                "Credenciales inválidas.");
            return response;
        }

        var check = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false)
            .ConfigureAwait(false);
        if (!check.Succeeded)
        {
            ServiceResponseBuilder.ApplyDtoFailure(
                response,
                401,
                "Credenciales inválidas.",
                "Credenciales inválidas.");
            return response;
        }

        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        var issuer = jwtSection["Issuer"] ?? "BookService";
        var audience = jwtSection["Audience"] ?? "BookServiceClients";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        response.IsSuccessful = true;
        response.Status = 200;
        response.Result = 1;
        response.Message = "Autenticación correcta.";
        response.Data = new LoginTokenDataDto
        {
            AccessToken = tokenString,
            TokenType = "Bearer",
            ExpiresInSeconds = 3600,
            ExpiresAtUtc = expires
        };
        response.Errors.Clear();
        return response;
    }
}
