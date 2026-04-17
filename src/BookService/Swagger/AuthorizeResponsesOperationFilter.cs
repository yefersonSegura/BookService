using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BS.WebAPI.Swagger;

/// <summary>
/// Documenta respuestas 401/403 en operaciones protegidas con <see cref="AuthorizeAttribute"/>.
/// </summary>
internal sealed class AuthorizeResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!RequiresAuth(context))
        {
            return;
        }

        operation.Responses ??= new OpenApiResponses();

        operation.Responses.TryAdd(
            "401",
            new OpenApiResponse { Description = "No autenticado. Envíe un JWT válido en el encabezado Authorization." });

        operation.Responses.TryAdd(
            "403",
            new OpenApiResponse { Description = "Autenticado pero sin permisos para este recurso." });
    }

    private static bool RequiresAuth(OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return false;
        }

        var actionAttrs = descriptor.MethodInfo.GetCustomAttributes(inherit: true);
        if (actionAttrs.OfType<IAllowAnonymous>().Any())
        {
            return false;
        }

        if (actionAttrs.OfType<AuthorizeAttribute>().Any())
        {
            return true;
        }

        return descriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true).OfType<AuthorizeAttribute>().Any();
    }
}
