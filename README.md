# Sistema de Renta de Películas — Sakila

Aplicación web desarrollada con **ASP.NET Core MVC**, **ADO.NET** y **SQL Server**, utilizando la base de datos Sakila como fuente de datos.

---

## Descripción

Sistema de renta de películas que implementa autenticación por roles, gestión de películas e inventario, alquiler y devolución de películas, con reglas de negocio y protección de contraseñas mediante SHA-256.

Roles disponibles:
- **Administrador**: gestiona películas, inventario y consulta alquileres.
- **Cliente**: consulta el catálogo, alquila películas y registra devoluciones.

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local o remoto) con la base de datos **Sakila** instalada
- Visual Studio 2022 o superior (opcional, puede usarse VS Code)

---

## Configuración

### 1. Ejecutar el script SQL

Ejecutar el archivo `Scripts/sakila_modificaciones.sql` sobre la base de datos Sakila en SQL Server.

Este script:
- Agrega la columna `activo` a la tabla `film` (eliminación lógica).
- Crea la tabla `Usuarios` para autenticación con hash SHA-256.

### 2. Configurar la cadena de conexión

Editar `appsettings.json` y ajustar `SakilaConnection` según la instancia de SQL Server:

```json
"ConnectionStrings": {
  "SakilaConnection": "Server=localhost;Database=sakila;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Ejemplos comunes:
- SQL Server Express: `Server=.\SQLEXPRESS;Database=sakila;...`
- Con credenciales: `Server=localhost;Database=sakila;User Id=sa;Password=tu_password;TrustServerCertificate=True;`

### 3. Ejecutar la aplicación

```bash
dotnet run
```

Al arrancar por primera vez la aplicación inserta los usuarios de prueba en la tabla `Usuarios` con contraseñas protegidas mediante hash SHA-256.

---

## Credenciales de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@sakila.com | Admin1234 |
| Cliente | customer@sakila.com | Customer1234 |

---

## Estructura del proyecto

```
ProyectoU1/
├── Controllers/    # AccountController, PeliculasController, InventarioController,
│                   # AlquilerController, ReportesController
├── Filters/        # RolRequeridoAttribute (base), SoloAutenticadoAttribute,
│                   # SoloAdminAttribute, SoloClienteAttribute
├── Helpers/        # SecurityHelper (SHA-256), SessionHelper
├── Models/         # Pelicula, Inventario, Alquiler, UsuarioSesion,
│                   # LoginViewModel, ReportePelicula
├── Views/          # Vistas por controlador + Shared (Layout, Partial Views)
├── Scripts/        # sakila_modificaciones.sql
├── wwwroot/        # CSS, JS, Bootstrap
├── appsettings.json
└── Program.cs      # Configuración, Session y seed de usuarios
```

---

## Funcionalidades implementadas

- Inicio y cierre de sesión con autenticación por hash SHA-256
- Navegación adaptada al rol del usuario
- Restricción de acceso mediante filtros de autorización (`RolRequeridoAttribute`)
- Catálogo de películas con disponibilidad en tiempo real
- CRUD completo de películas (solo Administrador)
- Gestión de inventario: agregar y eliminar copias
- Alquiler de películas con validación de disponibilidad y transacción
- Mis alquileres y devolución de películas
- Reporte de las 10 películas más alquiladas
- Eliminación lógica en `film` (campo `activo`) — Reto 2
- Contraseñas almacenadas como hash SHA-256 — Reto 1
- Session para mantener datos del usuario autenticado

---

## Retos técnicos

### Reto 1 — Contraseña protegida
Las contraseñas se almacenan como hash SHA-256 en `password_hash` de la tabla `Usuarios`. Al iniciar sesión la aplicación calcula el hash de la contraseña ingresada y lo compara con el almacenado. Nunca se guarda ni compara texto plano.

### Reto 2 — Eliminación lógica en `film`
La tabla `film` utiliza el campo `activo BIT`. Al desactivar una película el registro permanece en la base de datos conservando el historial de alquileres, pero deja de aparecer en el catálogo y no puede ser alquilada. El administrador puede restaurarla desde el panel de gestión.
