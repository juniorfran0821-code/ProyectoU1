using ProyectoU1.Models;
using System.Text.Json;

namespace ProyectoU1.Helpers
{
    public static class SessionHelper
    {
        private const string ClaveUsuario = "UsuarioActual";

        public static void GuardarUsuario(this ISession session, UsuarioSesion usuario)
        {
            string json = JsonSerializer.Serialize(usuario);
            session.SetString(ClaveUsuario, json);
        }

        public static UsuarioSesion? ObtenerUsuario(this ISession session)
        {
            string? json = session.GetString(ClaveUsuario);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<UsuarioSesion>(json);
        }
    }
}
