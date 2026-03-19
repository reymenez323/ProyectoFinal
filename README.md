# ProyectoFinal

Proyecto en **.NET 8 + SQL Server (LocalDB / SSMS)** que integra los requisitos de las prácticas **5, 6, 7 y 8**:

- API REST con CRUD de usuarios.
- Validación de correo único.
- Entity Framework Core con enfoque Code First.
- Autenticación con JWT.
- Endpoint para refrescar token.
- CRUD completo de productos, categorías y proveedores.
- Endpoints de agregación con LINQ.
- Registro de nuevos usuarios en archivo de texto con formato JSON.
- Endpoint para consultar el historial de logs.

## Estructura

- `ProyectoFinal/Controllers`
- `ProyectoFinal/Data`
- `ProyectoFinal/Models`
- `ProyectoFinal/DTOs`
- `ProyectoFinal/Services`
- `ProyectoFinal/Properties`

## Requisitos previos

- Visual Studio 2022 o VS Code
- .NET 8 SDK
- SQL Server LocalDB
- SQL Server Management Studio (SSMS)

## Configuración

La cadena de conexión está en `ProyectoFinal/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ProyectoFinalDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## Paquetes NuGet necesarios

Instala o restaura estos paquetes:

```bash
dotnet restore
```

## Migraciones Code First

Desde la carpeta del proyecto (`ProyectoFinal/ProyectoFinal`) ejecuta:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Si trabajas desde Visual Studio, en Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

## Ejecutar el proyecto

```bash
dotnet run
```

Swagger abrirá en una URL similar a:

- `https://localhost:7077/swagger`
- `http://localhost:5152/swagger`

## Autenticación JWT

### Registrar usuario

`POST /api/auth/register`

```json
{
  "nombre": "Juan Pérez",
  "correo": "juan@example.com",
  "fechaDeNacimiento": "2000-05-10T00:00:00",
  "passwordHash": "123456"
}
```

### Iniciar sesión

`POST /api/auth/login`

```json
{
  "correo": "juan@example.com",
  "password": "123456"
}
```

### Refrescar token

`POST /api/auth/refresh`

```json
{
  "refreshToken": "TOKEN_DEVUELTO_EN_LOGIN"
}
```

En Swagger, usa el botón **Authorize** y coloca:

```text
Bearer TU_TOKEN
```

## Endpoints principales

### Usuarios

- `GET /api/usuarios`
- `GET /api/usuarios/{id}`
- `POST /api/usuarios`
- `PUT /api/usuarios/{id}`
- `DELETE /api/usuarios/{id}`
- `GET /api/usuarios/logs`

### Categorías

- `GET /api/categorias`
- `GET /api/categorias/{id}`
- `POST /api/categorias`
- `PUT /api/categorias/{id}`
- `DELETE /api/categorias/{id}`

### Proveedores

- `GET /api/proveedores`
- `GET /api/proveedores/{id}`
- `POST /api/proveedores`
- `PUT /api/proveedores/{id}`
- `DELETE /api/proveedores/{id}`

### Productos

- `GET /api/productos`
- `GET /api/productos/{id}`
- `POST /api/productos`
- `PUT /api/productos/{id}`
- `DELETE /api/productos/{id}`
- `GET /api/productos/estadisticas`
- `GET /api/productos/por-categoria/{id}`
- `GET /api/productos/por-proveedor/{id}`
- `GET /api/productos/cantidad-total`

## Logs de usuarios

Cada vez que se registra un usuario, se agrega una línea JSON al archivo:

```text
ProyectoFinal/Logs/usuarios-log.txt
```

El endpoint `GET /api/usuarios/logs` devuelve el historial en formato JSON.
