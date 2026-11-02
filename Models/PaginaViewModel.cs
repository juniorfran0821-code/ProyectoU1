namespace ProyectoU1.Models
{
    public class PaginaViewModel<T>
    {
        public IEnumerable<T> Elementos { get; set; } = new List<T>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
