using System.Linq;
using BS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BS.WebAPI.Identity;

/// <summary>
/// Crea usuarios iniciales cuando la base está vacía.
/// Las contraseñas en texto plano solo existen en memoria aquí; en base de datos se guarda el
/// <strong>hash</strong> generado por <see cref="IPasswordHasher{ApplicationUser}"/> (unidireccional, no es cifrado reversible).
/// </summary>
internal static class IdentityDataSeeder
{
    private const string AdminPassword = "BookAdmin01!";
    private const string BibliotecarioPassword = "BiblioUser01!";

    public static async Task SeedAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@books.local",
            EmailConfirmed = true
        };
        var adminResult = await CreateUserWithHashedPasswordAsync(
            userManager,
            passwordHasher,
            admin,
            AdminPassword).ConfigureAwait(false);
        EnsureIdentitySucceeded(adminResult, "admin");

        var bibliotecario = new ApplicationUser
        {
            UserName = "bibliotecario",
            Email = "biblio@books.local",
            EmailConfirmed = true
        };
        var biblioResult = await CreateUserWithHashedPasswordAsync(
            userManager,
            passwordHasher,
            bibliotecario,
            BibliotecarioPassword).ConfigureAwait(false);
        EnsureIdentitySucceeded(biblioResult, "bibliotecario");
    }

    /// <summary>
    /// Persiste el usuario con la propiedad <c>PasswordHash</c> ya calculada (mismo algoritmo que el login).
    /// </summary>
    private static async Task<IdentityResult> CreateUserWithHashedPasswordAsync(
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        ApplicationUser user,
        string plainPassword)
    {
        user.PasswordHash = passwordHasher.HashPassword(user, plainPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("D");
        return await userManager.CreateAsync(user).ConfigureAwait(false);
    }

    private static void EnsureIdentitySucceeded(IdentityResult result, string userLabel)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        throw new InvalidOperationException($"No se pudo crear el usuario '{userLabel}': {errors}");
    }
}
