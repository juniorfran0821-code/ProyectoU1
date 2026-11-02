using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoU1.Filters;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers
{
    [SoloAdmin]
    public class InventarioController : Controller
    {
        private readonly string _connectionString;

        public InventarioController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SakilaConnection")!;
        }

        public IActionResult Index(int filmId)
        {
            var copias = new List<Inventario>();
            string tituloPelicula = string.Empty;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlFilm = "SELECT title FROM film WHERE film_id = @FilmId AND activo = 1";
            using (var cmdFilm = new SqlCommand(sqlFilm, connection))
            {
                cmdFilm.Parameters.AddWithValue("@FilmId", filmId);
                var resultado = cmdFilm.ExecuteScalar();
                if (resultado == null)
                    return NotFound();

                tituloPelicula = resultado.ToString()!;
            }

            const string sql = @"
                SELECT
                    i.inventory_id,
                    i.film_id,
                    i.store_id,
                    CASE
                        WHEN EXISTS (
                            SELECT 1 FROM rental r
                            WHERE r.inventory_id = i.inventory_id
                              AND r.return_date IS NULL
                        ) THEN 0
                        ELSE 1
                    END AS disponible
                FROM inventory i
                WHERE i.film_id = @FilmId
                ORDER BY i.inventory_id";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", filmId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                copias.Add(new Inventario
                {
                    InventoryId = reader.GetInt32(reader.GetOrdinal("inventory_id")),
                    FilmId = reader.GetInt32(reader.GetOrdinal("film_id")),
                    StoreId = reader.GetInt32(reader.GetOrdinal("store_id")),
                    TituloPelicula = tituloPelicula,
                    Disponible = reader.GetInt32(reader.GetOrdinal("disponible")) == 1
                });
            }

            ViewBag.FilmId = filmId;
            ViewBag.TituloPelicula = tituloPelicula;
            return View(copias);
        }

        public IActionResult Agregar(int filmId)
        {
            string tituloPelicula = ObtenerTitulo(filmId);
            if (string.IsNullOrEmpty(tituloPelicula))
                return NotFound();

            var inventario = new Inventario
            {
                FilmId = filmId,
                TituloPelicula = tituloPelicula,
                StoreId = 1
            };

            return View(inventario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(Inventario modelo)
        {
            if (!ModelState.IsValid)
            {
                modelo.TituloPelicula = ObtenerTitulo(modelo.FilmId);
                return View(modelo);
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                INSERT INTO inventory (film_id, store_id, last_update)
                VALUES (@FilmId, @StoreId, GETDATE())";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", modelo.FilmId);
            command.Parameters.AddWithValue("@StoreId", modelo.StoreId);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Copia agregada al inventario correctamente.";
            return RedirectToAction(nameof(Index), new { filmId = modelo.FilmId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int inventoryId, int filmId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlVerificar = @"
                SELECT COUNT(1) FROM rental
                WHERE inventory_id = @InventoryId";

            using var cmdVerificar = new SqlCommand(sqlVerificar, connection);
            cmdVerificar.Parameters.AddWithValue("@InventoryId", inventoryId);
            int totalAlquileres = (int)cmdVerificar.ExecuteScalar();

            if (totalAlquileres > 0)
            {
                TempData["Error"] = "No se puede eliminar esta copia porque tiene alquileres registrados en el historial.";
                return RedirectToAction(nameof(Index), new { filmId });
            }

            const string sql = "DELETE FROM inventory WHERE inventory_id = @InventoryId";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@InventoryId", inventoryId);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Copia eliminada del inventario correctamente.";
            return RedirectToAction(nameof(Index), new { filmId });
        }

        private string ObtenerTitulo(int filmId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "SELECT title FROM film WHERE film_id = @FilmId AND activo = 1";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", filmId);
            return command.ExecuteScalar()?.ToString() ?? string.Empty;
        }
    }
}
