USE sakila;
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'film' AND COLUMN_NAME = 'activo'
)
BEGIN
    ALTER TABLE film ADD activo BIT NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'Usuarios'
)
BEGIN
    CREATE TABLE Usuarios (
        usuario_id    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        nombre        NVARCHAR(100) NOT NULL,
        email         NVARCHAR(150) NOT NULL UNIQUE,
        password_hash NVARCHAR(64)  NOT NULL,
        rol           NVARCHAR(20)  NOT NULL,
        customer_id   INT           NULL,
        staff_id      TINYINT       NULL,
        activo        BIT           NOT NULL DEFAULT 1
    );
END
GO

-- Las 10 películas con mayor cantidad de alquileres
SELECT TOP 10
    f.title            AS Pelicula,
    COUNT(r.rental_id) AS TotalAlquileres
FROM film f
INNER JOIN inventory i ON f.film_id = i.film_id
INNER JOIN rental r    ON i.inventory_id = r.inventory_id
GROUP BY f.film_id, f.title
ORDER BY TotalAlquileres DESC;
GO

-- Las 10 películas que actualmente tengan al menos una copia
-- disponible para alquilar
SELECT TOP 10
    f.film_id,
    f.title              AS Pelicula,
    COUNT(i.inventory_id) AS CopiasDisponibles
FROM film f
INNER JOIN inventory i ON f.film_id = i.film_id
WHERE i.inventory_id NOT IN (
    SELECT inventory_id FROM rental WHERE return_date IS NULL
)
GROUP BY f.film_id, f.title
HAVING COUNT(i.inventory_id) >= 1
ORDER BY CopiasDisponibles DESC;
GO

-- Los 10 clientes con mayor cantidad de películas devueltas
SELECT TOP 10
    c.customer_id,
    c.first_name + ' ' + c.last_name AS NombreCompleto,
    COUNT(r.rental_id)               AS TotalDevoluciones
FROM customer c
INNER JOIN rental r ON c.customer_id = r.customer_id
WHERE r.return_date IS NOT NULL
GROUP BY c.customer_id, c.first_name, c.last_name
ORDER BY TotalDevoluciones DESC;
GO



SELECT
    u.usuario_id,
    u.nombre,
    u.email,
    u.rol,
    u.customer_id,
    u.staff_id,
    u.activo
FROM Usuarios u;
GO
