# Sistema de Renta de Películas con Sakila

## Descripción

Aplicación web desarrollada con **ASP.NET Core MVC**, **ADO.NET** (Microsoft.Data.SqlClient) y **SQL Server**, que usa la base de datos **Sakila** para simular un pequeño sistema de renta de películas.

Tiene dos roles:

- **Administrador:** gestiona películas y copias del inventario, y consulta los alquileres.
- **Cliente:** consulta el catálogo, alquila películas, revisa sus alquileres y las devuelve.

Incluye inicio y cierre de sesión con Session, autorización por roles mediante filtros, reglas de negocio (disponibilidad, propiedad y estado del alquiler), validaciones, un reporte de las películas más alquiladas y dos retos técnicos: contraseñas protegidas con hash SHA-256 y eliminación lógica de películas (`film.activo`).

## Requisitos

- .NET 10 SDK
- SQL Server con la base de datos **Sakila** ya instalada
- SQL Server Management Studio (SSMS) o similar para ejecutar el script
- Visual Studio compatible con .NET 10 

## Cómo ejecutarlo

1. **Clonar el repositorio**

   ```bash
   git clone https://github.com/juniorfran0821-code/ProyectoU1.git
   cd ProyectoU1
   ```

2. **Ejecutar el script SQL.** Abrir `Script/sakila_script.sql` en SSMS y ejecutarlo sobre la base de datos `sakila`. El script agrega la columna `activo` a `film` (eliminación lógica) y crea la tabla `Usuarios`.

3. **Revisar la cadena de conexión.** En `appsettings.json`, ajustar `SakilaConnection` si la instancia de SQL Server es distinta:

   ```json
   "SakilaConnection": "Server=.\\SQLEXPRESS;Database=sakila;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

4. **Ejecutar la aplicación**

   Abrir en el navegador la dirección que indique la consola (por ejemplo `https://localhost:7106`). Al arrancar por primera vez, la aplicación crea los usuarios de prueba en la tabla `Usuarios`.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@sakila.com | Admin1234 |
| Cliente | customer@sakila.com | Customer1234 |

Son cuentas de prueba creadas para el proyecto. Las contraseñas se guardan como hash SHA-256, no como texto.

## Estructura

- `Controllers/`: controladores (cuenta, películas, inventario, alquileres y reportes).
- `Models/`: modelos con validaciones (DataAnnotations).
- `Views/`: vistas Razor y Partial Views (`Views/Shared`).
- `Filters/`: filtros de autorización por rol.
- `Helpers/`: manejo de Session y hash de contraseñas.
- `Script/`: script SQL con las modificaciones sobre Sakila.
- `Program.cs`: configuración de la aplicación, Session y usuarios de prueba.
