namespace ProyectoU1.Filters
{
    public class SoloClienteAttribute : RolRequeridoAttribute
    {
        public SoloClienteAttribute() : base(rol: "Cliente") { }
    }
}
