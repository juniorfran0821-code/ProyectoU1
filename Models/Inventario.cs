using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models
{
    public class Inventario
    {
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "La película es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La película es obligatoria.")]
        [Display(Name = "Película")]
        public int FilmId { get; set; }

        [Display(Name = "Película")]
        public string TituloPelicula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tienda es obligatoria.")]
        [Range(1, 2, ErrorMessage = "La tienda debe ser 1 o 2.")]
        [Display(Name = "Tienda")]
        public int StoreId { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; }
    }
}
