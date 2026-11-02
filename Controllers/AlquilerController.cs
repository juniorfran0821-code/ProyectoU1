using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoU1.Filters;
using ProyectoU1.Helpers;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers
{
    [SoloAutenticado]
    public class AlquilerController : Controller
    {
        private readonly string _connectionString;

        public AlquilerController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SakilaConnection")!;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloCliente]
        public IActionResult Alquilar(int filmId)
        {
            var usuario = HttpContext.Session.ObtenerUsuario()!;

            if (!usuario.CustomerId.HasValue)
            {
                TempData["Error"] = "Su cuenta no está asociada a un cliente de Sakila.";
                return RedirectToAction("Catalogo", "Peliculas");
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sqlCopia = @"
                    SELECT TOP 1 i.inventory_id
                    FROM inventory i
                    INNER JOIN film f ON f.film_id = i.film_id
                    WHERE i.film_id = @FilmId
                      AND f.activo = 1
                      AND i.inventory_id NOT IN (
                          SELECT inventory_id FROM rental WHERE return_date IS NULL
                      )
                    ORDER BY i.inventory_id";

                using var cmdCopia = new SqlCommand(sqlCopia, connection, transaction);
                cmdCopia.Parameters.AddWithValue("@FilmId", filmId);
                var copiaResult = cmdCopia.ExecuteScalar();

                if (copiaResult == null)
                {
                    transaction.Rollback();
                    TempData["Error"] = "No hay copias disponibles para esta película en este momento.";
                    return RedirectToAction("Catalogo", "Peliculas");
                }

                int inventoryId = (int)copiaResult;

                const string sqlAlquiler = @"
                    INSERT INTO rental (rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
                    VALUES (GETDATE(), @InventoryId, @CustomerId, NULL, 1, GETDATE())";

                using var cmdAlquiler = new SqlCommand(sqlAlquiler, connection, transaction);
                cmdAlquiler.Parameters.AddWithValue("@InventoryId", inventoryId);
                cmdAlquiler.Parameters.AddWithValue("@CustomerId", usuario.CustomerId.Value);
                cmdAlquiler.ExecuteNonQuery();

                transaction.Commit();
                TempData["Mensaje"] = "Película alquilada exitosamente.";
                return RedirectToAction(nameof(MisAlquileres));
            }
            catch
            {
                transaction.Rollback();
                TempData["Error"] = "No se pudo registrar el alquiler. Intente nuevamente.";
                return RedirectToAction("Catalogo", "Peliculas");
            }
        }

        [SoloCliente]
        public IActionResult MisAlquileres(int pagina = 1)
        {
            var usuario = HttpContext.Session.ObtenerUsuario()!;

            if (!usuario.CustomerId.HasValue)
            {
                TempData["Error"] = "Su cuenta no está asociada a un cliente de Sakila.";
                return RedirectToAction("Catalogo", "Peliculas");
            }

            const int porPagina = 10;
            var alquileres = new List<Alquiler>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlTotal = "SELECT COUNT(*) FROM rental WHERE customer_id = @CustomerId";
            using var cmdTotal = new SqlCommand(sqlTotal, connection);
            cmdTotal.Parameters.AddWithValue("@CustomerId", usuario.CustomerId.Value);
            int total = (int)cmdTotal.ExecuteScalar();

            const string sql = @"
                SELECT
                    r.rental_id,
                    r.rental_date,
                    r.return_date,
                    r.inventory_id,
                    r.customer_id,
                    r.staff_id,
                    f.film_id,
                    f.title AS titulo_pelicula,
                    c.first_name + ' ' + c.last_name AS nombre_cliente
                FROM rental r
                INNER JOIN inventory i ON r.inventory_id = i.inventory_id
                INNER JOIN film f      ON i.film_id = f.film_id
                INNER JOIN customer c  ON r.customer_id = c.customer_id
                WHERE r.customer_id = @CustomerId
                ORDER BY r.rental_date DESC
                OFFSET @Offset ROWS FETCH NEXT @PorPagina ROWS ONLY";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@CustomerId", usuario.CustomerId.Value);
            command.Parameters.AddWithValue("@Offset", (pagina - 1) * porPagina);
            command.Parameters.AddWithValue("@PorPagina", porPagina);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                alquileres.Add(MapearAlquiler(reader));

            ViewBag.PaginaActual         = pagina;
            ViewBag.TotalPaginas         = (int)Math.Ceiling((double)total / porPagina);
            ViewBag.AccionPaginacion      = "MisAlquileres";
            ViewBag.ControladorPaginacion = "Alquiler";
            return View(alquileres);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloCliente]
        public IActionResult Devolver(int rentalId)
        {
            var usuario = HttpContext.Session.ObtenerUsuario()!;

            if (!usuario.CustomerId.HasValue)
            {
                TempData["Error"] = "Su cuenta no está asociada a un cliente de Sakila.";
                return RedirectToAction("Catalogo", "Peliculas");
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlVerificar = @"
                SELECT customer_id, return_date
                FROM rental
                WHERE rental_id = @RentalId";

            using var cmdVerificar = new SqlCommand(sqlVerificar, connection);
            cmdVerificar.Parameters.AddWithValue("@RentalId", rentalId);

            using var reader = cmdVerificar.ExecuteReader();
            if (!reader.Read())
                return NotFound();

            int customerId = reader.GetInt32(reader.GetOrdinal("customer_id"));
            bool estaDevuelto = !reader.IsDBNull(reader.GetOrdinal("return_date"));
            reader.Close();

            if (customerId != usuario.CustomerId.Value)
            {
                TempData["Error"] = "No puede devolver un alquiler que no le pertenece.";
                return RedirectToAction(nameof(MisAlquileres));
            }

            if (estaDevuelto)
            {
                TempData["Error"] = "Este alquiler ya fue devuelto anteriormente.";
                return RedirectToAction(nameof(MisAlquileres));
            }

            const string sqlDevolver = @"
                UPDATE rental
                SET return_date = GETDATE(), last_update = GETDATE()
                WHERE rental_id = @RentalId";

            using var cmdDevolver = new SqlCommand(sqlDevolver, connection);
            cmdDevolver.Parameters.AddWithValue("@RentalId", rentalId);
            cmdDevolver.ExecuteNonQuery();

            TempData["Mensaje"] = "Película devuelta correctamente.";
            return RedirectToAction(nameof(MisAlquileres));
        }

        [SoloAdmin]
        public IActionResult Todos(int pagina = 1)
        {
            const int porPagina = 15;
            var alquileres = new List<Alquiler>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlTotal = "SELECT COUNT(*) FROM rental";
            using var cmdTotal = new SqlCommand(sqlTotal, connection);
            int total = (int)cmdTotal.ExecuteScalar();

            const string sql = @"
                SELECT
                    r.rental_id,
                    r.rental_date,
                    r.return_date,
                    r.inventory_id,
                    r.customer_id,
                    r.staff_id,
                    f.film_id,
                    f.title AS titulo_pelicula,
                    c.first_name + ' ' + c.last_name AS nombre_cliente
                FROM rental r
                INNER JOIN inventory i ON r.inventory_id = i.inventory_id
                INNER JOIN film f      ON i.film_id = f.film_id
                INNER JOIN customer c  ON r.customer_id = c.customer_id
                ORDER BY r.rental_date DESC
                OFFSET @Offset ROWS FETCH NEXT @PorPagina ROWS ONLY";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Offset", (pagina - 1) * porPagina);
            command.Parameters.AddWithValue("@PorPagina", porPagina);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                alquileres.Add(MapearAlquiler(reader));

            ViewBag.PaginaActual         = pagina;
            ViewBag.TotalPaginas         = (int)Math.Ceiling((double)total / porPagina);
            ViewBag.AccionPaginacion      = "Todos";
            ViewBag.ControladorPaginacion = "Alquiler";
            return View(alquileres);
        }

        private static Alquiler MapearAlquiler(SqlDataReader reader)
        {
            return new Alquiler
            {
                RentalId = reader.GetInt32(reader.GetOrdinal("rental_id")),
                RentalDate = reader.GetDateTime(reader.GetOrdinal("rental_date")),
                ReturnDate = reader.IsDBNull(reader.GetOrdinal("return_date"))
                                     ? null
                                     : reader.GetDateTime(reader.GetOrdinal("return_date")),
                InventoryId = reader.GetInt32(reader.GetOrdinal("inventory_id")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                StaffId = (int)reader.GetByte(reader.GetOrdinal("staff_id")),
                FilmId = reader.GetInt32(reader.GetOrdinal("film_id")),
                TituloPelicula = reader.GetString(reader.GetOrdinal("titulo_pelicula")),
                NombreCliente = reader.GetString(reader.GetOrdinal("nombre_cliente"))
            };
        }
    }
}
