# BookService

API REST (.NET / ASP.NET Core) para gestión de libros y autores, con autenticación JWT.

## Docker

La imagen se construye en **dos etapas**: compilación con `mcr.microsoft.com/dotnet/sdk` y ejecución con `mcr.microsoft.com/dotnet/aspnet` (runtime compatible con .NET 7+; este proyecto usa **.NET 10**).

### Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recomendado en **Windows 11** y **macOS**), o  
- **Docker Engine** en **Linux** (paquete de la distribución o instalación oficial).

### 1. Construir la imagen

En la **raíz del repositorio** (donde está el `Dockerfile`):

```bash
docker build -t book-service .
```

El tag `book-service` es arbitrario; puede cambiarlo por otro nombre.

### 2. Ejecutar el contenedor

La aplicación escucha en el **puerto 8080** dentro del contenedor. Publíquelo en el host para acceder desde el navegador o herramientas HTTP.

#### Linux (terminal)

```bash
docker run --rm -p 8080:8080 book-service
```

#### macOS (Terminal; Docker Desktop)

```bash
docker run --rm -p 8080:8080 book-service
```

#### Windows 11 (PowerShell o CMD; Docker Desktop)

```powershell
docker run --rm -p 8080:8080 book-service
```

Con esto, la API queda disponible en el equipo anfitrión en:

- **http://localhost:8080**
- **Swagger UI:** la imagen Docker tiene Swagger activado; abra **http://localhost:8080/swagger**

### Variables de entorno opcionales

| Variable | Descripción |
|----------|-------------|
| `Swagger__Enabled` | `true` (valor por defecto en la imagen Docker) expone Swagger en `/swagger`. Ponga `false` para desactivarlo. |
| `ASPNETCORE_ENVIRONMENT` | `Development` también habilita Swagger (además de otras características de desarrollo). |
| `ASPNETCORE_URLS` | Por defecto la imagen usa `http://+:8080`. |

### Base de datos (SQLite)

Por defecto se usa SQLite con el archivo `books.db` en el directorio de trabajo del contenedor (`/app`). Si desea **persistir** datos entre reinicios del contenedor, monte un volumen:

```bash
docker run --rm -p 8080:8080 -v book-service-data:/app book-service
```

### Puerto distinto en el host

Para usar el puerto **9090** en el máquina anfitriona:

```bash
docker run --rm -p 9090:8080 book-service
```

La API seguirá escuchando en 8080 **dentro** del contenedor; el mapeo `9090:8080` la expone en `http://localhost:9090` en el host.
