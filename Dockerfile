# Etapa 1: compilar y publicar la API con el SDK (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos referenciados y restaurar dependencias (mejor uso de caché de capas)
COPY ["src/BookService/BS.WebAPI.csproj", "BookService/"]
COPY ["src/BS.Application/BS.Application.csproj", "BS.Application/"]
COPY ["src/BS.Domain/BS.Domain.csproj", "BS.Domain/"]
COPY ["src/BS.Infrastructure/BS.Infrastructure.csproj", "BS.Infrastructure/"]

RUN dotnet restore "BookService/BS.WebAPI.csproj"

# Código fuente y publicación en modo Release
COPY src/ .
RUN dotnet publish "BookService/BS.WebAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 2: imagen final solo con runtime ASP.NET Core (más pequeña)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
# Habilita Swagger UI en /swagger sin usar entorno Development
ENV Swagger__Enabled=true
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BS.WebAPI.dll"]
