using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoU1.Helpers;

namespace ProyectoU1.Filters
{
    public class RolRequeridoAttribute : ActionFilterAttribute
    {
        private readonly string? _rol;

        public RolRequeridoAttribute(string? rol = null)
        {
            _rol = rol;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuario = context.HttpContext.Session.ObtenerUsuario();

            if (usuario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_rol != null && usuario.Rol != _rol)
            {
                context.Result = new RedirectToActionResult("Denegado", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
