using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models
{
    public class Alquiler
    {
        public int RentalId { get; set; }

        [Display(Name = "Fecha de alquiler")]
        public DateTime RentalDate { get; set; }

        [Display(Name = "Fecha de devolución")]
        public DateTime? ReturnDate { get; set; }

        public int InventoryId { get; set; }

        [Display(Name = "Cliente")]
        public int CustomerId { get; set; }

        [Display(Name = "Nombre del cliente")]
        public string NombreCliente { get; set; } = string.Empty;

        [Display(Name = "Atendido por")]
        public int StaffId { get; set; }

        [Display(Name = "Película")]
        public string TituloPelicula { get; set; } = string.Empty;

        public int FilmId { get; set; }

        [Display(Name = "Devuelto")]
        public bool EstaDevuelto => ReturnDate.HasValue;
    }
}
