using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProyectoU1.Filters;
using ProyectoU1.Helpers;
using ProyectoU1.Models;

namespace ProyectoU1.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SakilaConnection")!;
        }

        public IActionResult Login()
        {
            var usuarioActual = HttpContext.Session.ObtenerUsuario();
            if (usuarioActual != null)
                return RedirigirPorRol(usuarioActual.Rol);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
                SELECT usuario_id, nombre, email, password_hash, rol, customer_id, staff_id
                FROM Usuarios
                WHERE email = @Email AND activo = 1";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", modelo.Email);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View(modelo);
            }

            string hashAlmacenado = reader.GetString(reader.GetOrdinal("password_hash"));

            if (!SecurityHelper.VerifyPassword(modelo.Password, hashAlmacenado))
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View(modelo);
            }

            var usuario = new UsuarioSesion
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Rol = reader.GetString(reader.GetOrdinal("rol")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("customer_id"))
                                 ? null
                                 : (int?)reader.GetInt32(reader.GetOrdinal("customer_id")),
                StaffId = reader.IsDBNull(reader.GetOrdinal("staff_id"))
                                 ? null
                                 : (int?)reader.GetByte(reader.GetOrdinal("staff_id"))
            };

            HttpContext.Session.GuardarUsuario(usuario);

            return RedirigirPorRol(usuario.Rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SoloAutenticado]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Denegado()
        {
            return View();
        }

        private IActionResult RedirigirPorRol(string rol)
        {
            if (rol == "Administrador")
                return RedirectToAction("Index", "Peliculas");

            return RedirectToAction("Catalogo", "Peliculas");
        }
    }
}
