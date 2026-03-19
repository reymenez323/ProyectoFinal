# Auditoría rápida del repositorio original Practica7

## Incumplimientos detectados

1. **El modelo `Usuario` no cumple completamente la práctica 5**:
   - Tiene `Id`, `Nombre` y `Correo`, pero no incluye `FechaDeNacimiento`.
   - En cambio usa `PasswordHash`, `FechaDeRegistro`, `RefreshToken` y `RefreshTokenExpiryTime`.

2. **`UsuariosController` no implementa el CRUD exactamente como lo pide la práctica 5**:
   - `POST /api/usuarios` redirige a `Auth/Register` en vez de manejar directamente la creación.
   - El controlador está protegido con `[Authorize]`, lo que cambia el comportamiento esperado para la práctica básica.

3. **`ProveedoresController` está incompleto**:
   - Solo tiene una acción MVC `Index()` y no implementa CRUD de API.

4. **El README no coincide del todo con el estado real del código**:
   - El README afirma soporte completo de usuarios, productos, categorías y proveedores, pero el controlador de proveedores no cumple.

5. **El proyecto parece estar renombrado de forma inconsistente**:
   - El repositorio se llama `Practica7`, la carpeta del proyecto es `Practica5`, y el README sigue describiendo “Práctica 5”.

## Decisión aplicada en la nueva versión

Se generó una nueva estructura llamada **ProyectoFinal** con:
- `net8.0`
- SQL Server LocalDB / SSMS
- CRUD completo de usuarios, categorías, proveedores y productos
- JWT con login y refresh
- agregaciones de productos
- logs JSON de usuarios y endpoint para consultarlos
