using System.Reflection;
using System.Text;
using BS.Infrastructure.Repository;
using BS.Infrastructure.Services;
using BS.Application.Interfaces;
using BS.Application.Services;
using BS.Domain.Interfaces;
using BS.Infrastructure.Data;
using BS.WebAPI.ExceptionHandling;
using BS.WebAPI.Identity;
using BS.WebAPI.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://httpwg.org/specs/rfc9110.html#status.400",
                Title = "Errores de validación en la solicitud.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path.Value
            };
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(problem);
        };
    });
builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Configure Jwt:Key (mínimo 32 caracteres).");

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ILoginService, LoginService>();

var externalApis = builder.Configuration.GetSection("ExternalApis");
var isbnSoapBaseUrl = externalApis["IsbnSoapServiceBaseUrl"]
    ?? throw new InvalidOperationException("Configure ExternalApis:IsbnSoapServiceBaseUrl.");
var openLibraryBaseUrl = externalApis["OpenLibraryBaseUrl"]
    ?? throw new InvalidOperationException("Configure ExternalApis:OpenLibraryBaseUrl.");

builder.Services.AddHttpClient<IIsbnSoapValidator, IsbnSoapValidator>(client =>
{
    client.BaseAddress = new Uri(isbnSoapBaseUrl);
});

builder.Services.AddHttpClient<IOpenLibraryCoverClient, OpenLibraryCoverClient>(client =>
{
    client.BaseAddress = new Uri(openLibraryBaseUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookService API",
        Version = "v1",
        Description =
            "API REST para gestión de libros y autores. Autenticación con JWT (expiración 1 h). "
            + "Obtenga un token con **POST /api/auth/login** y envíelo como `Authorization: Bearer {token}`.",
        Contact = new OpenApiContact
        {
            Name = "BookService"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT emitido por **POST /api/auth/login**. Ejemplo: `Bearer eyJhbGciOi...`"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    options.OperationFilter<AuthorizeResponsesOperationFilter>();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
    await IdentityDataSeeder.SeedAsync(db, userManager, passwordHasher).ConfigureAwait(false);
}

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Swagger:Enabled", false);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookService API v1");
        c.DisplayRequestDuration();
    });
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
