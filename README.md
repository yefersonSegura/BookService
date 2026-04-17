# BookService

API REST en **ASP.NET Core** para gestión de libros y autores, con autenticación JWT, integración SOAP (validación ISBN), API REST (portadas Open Library) y carga masiva por CSV.

---

## Arquitectura y estructura del repositorio

Se sigue una **Clean Architecture sencilla**: las dependencias apuntan hacia el centro (dominio y casos de uso), y la infraestructura implementa los contratos sin invertir esa regla.

| Proyecto | Carpeta | Rol |
|----------|---------|-----|
| **BS.Domain** | `src/BS.Domain` | Entidades (`Book`, `Author`), consultas y **interfaces** de repositorios. Sin dependencias de EF ni HTTP. |
| **BS.Application** | `src/BS.Application` | Casos de uso (servicios), DTOs, validaciones con **Data Annotations**, normalización de texto, orquestación de reglas de negocio. Depende solo de Domain. |
| **BS.Infrastructure** | `src/BS.Infrastructure` | **EF Core** (`AppDbContext`, SQLite), implementaciones de repositorios, **ASP.NET Core Identity** (`ApplicationUser`), clientes HTTP (SOAP ISBN, Open Library), implementación de login JWT. |
| **BS.WebAPI** | `src/BookService` | **Controladores**, composición de DI (`Program.cs`), Swagger, JWT Bearer, **manejo global de excepciones** (`ProblemDetails`). |

Flujo típico: **Controller → servicio de aplicación → repositorio (interfaz) → implementación EF**. Los servicios externos se inyectan por interfaces definidas en Application e implementadas en Infrastructure.

---

## Funcionalidades y alcance

Resumen de lo que incluye el servicio y cómo está construido.

### Stack y persistencia

- **ASP.NET Core Web API** (versión **7 o superior**; este repo usa **.NET 10**).
- Patrón **repositorio** + capas tipo **Clean Architecture** (controlador → servicio → repositorio / contexto).
- **Entity Framework Core** con **SQLite** como motor de base de datos.

**Base de datos: SQLite — por qué**

Se eligió **SQLite** con **EF Core** por ser un motor **embebido y ligero**: no requiere **instalar ni administrar un servidor** de base de datos aparte, usa **un solo archivo** (`books.db` por defecto), es **portable** entre entornos y encaja bien con **Docker** (persistencia opcional montando un volumen). Es una opción habitual para APIs de tamaño medio, prototipos y despliegues sencillos. El proveedor `Microsoft.EntityFrameworkCore.Sqlite` permite mantener el mismo modelo de datos; si más adelante se necesita otro motor, suele bastar con **cambiar la cadena de conexión** y el proveedor de EF Core.

Cadena por defecto: `Data Source=books.db` (configurable en `ConnectionStrings:DefaultConnection`).

### Modelos de datos

- Entidades **Book** e **Author** con **Guid** generados; campos principales: ISBN, título, portada, año de publicación, relación con autor, nombre de autor y colección de libros del autor.
- Relación **Book → Author** con **eliminación en cascada** al borrar un autor (configurado en EF Core).

### Seguridad

- Autenticación **JWT Bearer**; emisión de token en **`POST /api/auth/login`**.
- **Expiración del token: 1 hora**.
- Usuarios con credenciales almacenadas vía **ASP.NET Core Identity** (contraseñas hasheadas); **usuarios precargados** al iniciar (sin API de administración de usuarios).
- Endpoints de **libros** y **autores** protegidos con **`[Authorize]`**; login público.

### Endpoints REST principales

| Área | Métodos | Descripción resumida |
|------|---------|----------------------|
| Auth | `POST /api/auth/login` | Obtener JWT |
| Libros | `GET/POST/PATCH/DELETE` bajo `/api/books`, `GET /api/books/{id}`, `GET /api/books/validation/{isbn}`, `POST /api/books/massive`, `POST /api/books/upload` | CRUD, validación ISBN, creación masiva y **carga CSV** |
| Autores | `GET/POST/PATCH/DELETE` bajo `/api/authors` | CRUD con paginación en listados |

Los listados de libros y autores incluyen **paginación**; en libros hay **búsqueda opcional** por título y nombre de autor según los parámetros de consulta.

### Integraciones externas

- **REST (portada):** `HttpClient` / `IHttpClientFactory` contra **Open Library** (`https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json`) para obtener la URL de portada.
- **SOAP (ISBN):** llamada al servicio documentado en el WSDL de **daehosting**; la lógica valida ISBN **10** y **13** según las operaciones del servicio.

### Validaciones y normalización

- **Data Annotations** en DTOs (longitudes y campos obligatorios).
- **Normalización** de `Title` (libro) y `Name` (autor) antes de persistir: mayúsculas, sin dígitos, sustitución de tildes y caracteres especiales, espacios colapsados.

### Carga masiva (CSV)

- **`POST /api/books/upload`**: archivo CSV con columnas **isbn, title, publicationYear, authorName**; misma lógica que el alta individual (ISBN, portada, normalización); **creación de autor** si no existe.

### Documentación y errores

- **Swagger / OpenAPI** (Swashbuckle), UI en `/swagger` cuando está habilitado.
- **Manejo centralizado de excepciones** que devuelve respuestas **ProblemDetails** coherentes.

### Pruebas

- Prueba **unitaria de servicio** con **mocking** del repositorio.
- Prueba **unitaria de controlador** con **mocking** del servicio de login.
- Prueba de **integración** con **WebApplicationFactory** (pipeline HTTP real).

### Despliegue con Docker

El repositorio incluye un **`Dockerfile` multi-etapa** (SDK + runtime `aspnet`). Más abajo se explica cómo **construir y ejecutar** la imagen en **Linux**, **macOS** y **Windows 11** (Docker Desktop o Docker Engine).

---

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
