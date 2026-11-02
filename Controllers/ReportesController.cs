using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoU1.Filters;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers
{
    [SoloAdmin]
    public class ReportesController : Controller
    {
        private readonly string _connectionString;

        public ReportesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SakilaConnection")!;
        }

        public IActionResult PeliculasMasAlquiladas()
        {
            var resultados = new List<ReportePelicula>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT TOP 10
                    f.title            AS Pelicula,
                    COUNT(r.rental_id) AS TotalAlquileres
                FROM film f
                INNER JOIN inventory i ON f.film_id = i.film_id
                INNER JOIN rental r    ON i.inventory_id = r.inventory_id
                GROUP BY f.film_id, f.title
                ORDER BY TotalAlquileres DESC";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                resultados.Add(new ReportePelicula
                {
                    Titulo = reader.GetString(reader.GetOrdinal("Pelicula")),
                    TotalAlquileres = reader.GetInt32(reader.GetOrdinal("TotalAlquileres"))
                });
            }

            return View(resultados);
        }
    }
}
