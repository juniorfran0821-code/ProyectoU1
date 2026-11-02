namespace ProyectoU1.Filters
{
    public class SoloAdminAttribute : RolRequeridoAttribute
    {
        public SoloAdminAttribute() : base(rol: "Administrador") { }
    }
}
