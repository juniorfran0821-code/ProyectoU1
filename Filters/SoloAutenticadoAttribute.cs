namespace ProyectoU1.Filters
{
    public class SoloAutenticadoAttribute : RolRequeridoAttribute
    {
        public SoloAutenticadoAttribute() : base(rol: null) { }
    }
}
