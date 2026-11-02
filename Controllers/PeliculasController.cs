using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoU1.Filters;
using ProyectoU1.Helpers;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly string _connectionString;

        public PeliculasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SakilaConnection")!;
        }

        [SoloAutenticado]
        public IActionResult Catalogo(int pagina = 1)
        {
            const int porPagina = 12;
            var peliculas = new List<Pelicula>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlTotal = "SELECT COUNT(*) FROM film WHERE activo = 1";
            using var cmdTotal = new SqlCommand(sqlTotal, connection);
            int total = (int)cmdTotal.ExecuteScalar();

            const string sql = @"
                SELECT
                    f.film_id,
                    f.title,
                    MAX(CAST(f.description AS NVARCHAR(MAX))) AS description,
                    f.release_year,
                    f.rental_duration,
                    f.rental_rate,
                    f.length,
                    f.replacement_cost,
                    f.rating,
                    f.activo,
                    COUNT(DISTINCT i.inventory_id) AS total_copias,
                    COUNT(DISTINCT CASE WHEN r.rental_id IS NULL THEN i.inventory_id END) AS copias_disponibles
                FROM film f
                LEFT JOIN inventory i ON f.film_id = i.film_id
                LEFT JOIN rental r    ON r.inventory_id = i.inventory_id AND r.return_date IS NULL
                WHERE f.activo = 1
                GROUP BY
                    f.film_id, f.title, f.release_year,
                    f.rental_duration, f.rental_rate, f.length,
                    f.replacement_cost, f.rating, f.activo
                ORDER BY f.title
                OFFSET @Offset ROWS FETCH NEXT @PorPagina ROWS ONLY";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Offset", (pagina - 1) * porPagina);
            command.Parameters.AddWithValue("@PorPagina", porPagina);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                peliculas.Add(MapearPelicula(reader));

            ViewBag.PaginaActual         = pagina;
            ViewBag.TotalPaginas         = (int)Math.Ceiling((double)total / porPagina);
            ViewBag.AccionPaginacion      = "Catalogo";
            ViewBag.ControladorPaginacion = "Peliculas";
            return View(peliculas);
        }

        [SoloAutenticado]
        public IActionResult Detalle(int id)
        {
            var pelicula = ObtenerPeliculaPorId(id);
            if (pelicula == null)
                return NotFound();

            var usuario = HttpContext.Session.ObtenerUsuario();
            if (!pelicula.Activo && usuario?.Rol != "Administrador")
                return NotFound();

            return View(pelicula);
        }

        [SoloAdmin]
        public IActionResult Index(int pagina = 1)
        {
            const int porPagina = 15;
            var peliculas = new List<Pelicula>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sqlTotal = "SELECT COUNT(*) FROM film";
            using var cmdTotal = new SqlCommand(sqlTotal, connection);
            int total = (int)cmdTotal.ExecuteScalar();

            const string sql = @"
                SELECT
                    f.film_id,
                    f.title,
                    MAX(CAST(f.description AS NVARCHAR(MAX))) AS description,
                    f.release_year,
                    f.rental_duration,
                    f.rental_rate,
                    f.length,
                    f.replacement_cost,
                    f.rating,
                    f.activo,
                    COUNT(DISTINCT i.inventory_id) AS total_copias,
                    COUNT(DISTINCT CASE WHEN r.rental_id IS NULL THEN i.inventory_id END) AS copias_disponibles
                FROM film f
                LEFT JOIN inventory i ON f.film_id = i.film_id
                LEFT JOIN rental r    ON r.inventory_id = i.inventory_id AND r.return_date IS NULL
                GROUP BY
                    f.film_id, f.title, f.release_year,
                    f.rental_duration, f.rental_rate, f.length,
                    f.replacement_cost, f.rating, f.activo
                ORDER BY f.activo DESC, f.title
                OFFSET @Offset ROWS FETCH NEXT @PorPagina ROWS ONLY";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Offset", (pagina - 1) * porPagina);
            command.Parameters.AddWithValue("@PorPagina", porPagina);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                peliculas.Add(MapearPelicula(reader));

            ViewBag.PaginaActual         = pagina;
            ViewBag.TotalPaginas         = (int)Math.Ceiling((double)total / porPagina);
            ViewBag.AccionPaginacion      = "Index";
            ViewBag.ControladorPaginacion = "Peliculas";
            return View(peliculas);
        }

        [SoloAdmin]
        public IActionResult Crear()
        {
            return View(new Pelicula { RentalDuration = 3, RentalRate = 2.99m, ReplacementCost = 19.99m });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloAdmin]
        public IActionResult Crear(Pelicula modelo)
        {
            ValidarDecimales(modelo);

            if (!ModelState.IsValid)
                return View(modelo);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                INSERT INTO film
                    (title, description, release_year, language_id, rental_duration,
                     rental_rate, length, replacement_cost, rating, activo, last_update)
                VALUES
                    (@Title, @Description, @ReleaseYear, 1, @RentalDuration,
                     @RentalRate, @Length, @ReplacementCost, @Rating, 1, GETDATE())";

            using var command = new SqlCommand(sql, connection);
            AgregarParametrosPelicula(command, modelo);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Película creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [SoloAdmin]
        public IActionResult Editar(int id)
        {
            var pelicula = ObtenerPeliculaPorId(id);
            if (pelicula == null)
                return NotFound();

            return View(pelicula);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloAdmin]
        public IActionResult Editar(int id, Pelicula modelo)
        {
            modelo.FilmId = id;
            ValidarDecimales(modelo);

            if (!ModelState.IsValid)
                return View(modelo);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                UPDATE film SET
                    title            = @Title,
                    description      = @Description,
                    release_year     = @ReleaseYear,
                    rental_duration  = @RentalDuration,
                    rental_rate      = @RentalRate,
                    length           = @Length,
                    replacement_cost = @ReplacementCost,
                    rating           = @Rating,
                    last_update      = GETDATE()
                WHERE film_id = @FilmId";

            using var command = new SqlCommand(sql, connection);
            AgregarParametrosPelicula(command, modelo);
            command.Parameters.AddWithValue("@FilmId", id);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Película actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloAdmin]
        public IActionResult Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "UPDATE film SET activo = 0, last_update = GETDATE() WHERE film_id = @FilmId";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", id);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Película desactivada. El historial de alquileres se conserva.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloAdmin]
        public IActionResult Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = "UPDATE film SET activo = 1, last_update = GETDATE() WHERE film_id = @FilmId";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", id);
            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Película restaurada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private Pelicula? ObtenerPeliculaPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT
                    f.film_id,
                    f.title,
                    MAX(CAST(f.description AS NVARCHAR(MAX))) AS description,
                    f.release_year,
                    f.rental_duration,
                    f.rental_rate,
                    f.length,
                    f.replacement_cost,
                    f.rating,
                    f.activo,
                    COUNT(DISTINCT i.inventory_id) AS total_copias,
                    COUNT(DISTINCT CASE WHEN r.rental_id IS NULL THEN i.inventory_id END) AS copias_disponibles
                FROM film f
                LEFT JOIN inventory i ON f.film_id = i.film_id
                LEFT JOIN rental r    ON r.inventory_id = i.inventory_id AND r.return_date IS NULL
                WHERE f.film_id = @FilmId
                GROUP BY
                    f.film_id, f.title, f.release_year,
                    f.rental_duration, f.rental_rate, f.length,
                    f.replacement_cost, f.rating, f.activo";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@FilmId", id);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return MapearPelicula(reader);
        }

        private static Pelicula MapearPelicula(SqlDataReader reader)
        {
            int? releaseYear = null;
            if (!reader.IsDBNull(reader.GetOrdinal("release_year")))
            {
                string yearStr = reader.GetString(reader.GetOrdinal("release_year"));
                if (int.TryParse(yearStr, out int parsedYear))
                    releaseYear = parsedYear;
            }

            return new Pelicula
            {
                FilmId = reader.GetInt32(reader.GetOrdinal("film_id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("description")),
                ReleaseYear = releaseYear,
                RentalDuration = reader.GetByte(reader.GetOrdinal("rental_duration")),
                RentalRate = reader.GetDecimal(reader.GetOrdinal("rental_rate")),
                Length = reader.IsDBNull(reader.GetOrdinal("length"))
                             ? null
                             : (int?)reader.GetInt16(reader.GetOrdinal("length")),
                ReplacementCost = reader.GetDecimal(reader.GetOrdinal("replacement_cost")),
                Rating = reader.IsDBNull(reader.GetOrdinal("rating"))
                             ? null
                             : reader.GetString(reader.GetOrdinal("rating")),
                Activo = reader.GetBoolean(reader.GetOrdinal("activo")),
                TotalCopias = reader.IsDBNull(reader.GetOrdinal("total_copias"))
                                  ? 0
                                  : Convert.ToInt32(reader["total_copias"]),
                CopiasDisponibles = reader.IsDBNull(reader.GetOrdinal("copias_disponibles"))
                                        ? 0
                                        : Convert.ToInt32(reader["copias_disponibles"])
            };
        }

        private void ValidarDecimales(Pelicula modelo)
        {
            if (modelo.RentalRate < 0.01m || modelo.RentalRate > 99.99m)
                ModelState.AddModelError(nameof(modelo.RentalRate),
                    "La tarifa debe estar entre 0,01 y 99,99.");

            if (modelo.ReplacementCost < 0.01m || modelo.ReplacementCost > 999.99m)
                ModelState.AddModelError(nameof(modelo.ReplacementCost),
                    "El costo debe estar entre 0,01 y 999,99.");
        }

        private static void AgregarParametrosPelicula(SqlCommand command, Pelicula modelo)
        {
            command.Parameters.AddWithValue("@Title", modelo.Title);
            command.Parameters.AddWithValue("@Description",
                string.IsNullOrWhiteSpace(modelo.Description) ? (object)DBNull.Value : modelo.Description);
            command.Parameters.AddWithValue("@ReleaseYear",
                modelo.ReleaseYear.HasValue ? (object)modelo.ReleaseYear.Value.ToString() : DBNull.Value);
            command.Parameters.AddWithValue("@RentalDuration", modelo.RentalDuration);
            command.Parameters.AddWithValue("@RentalRate", modelo.RentalRate);
            command.Parameters.AddWithValue("@Length",
                modelo.Length.HasValue ? (object)modelo.Length.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ReplacementCost", modelo.ReplacementCost);
            command.Parameters.AddWithValue("@Rating",
                string.IsNullOrWhiteSpace(modelo.Rating) ? (object)DBNull.Value : modelo.Rating);
        }
    }
}
