using System.ComponentModel.DataAnnotations;

namespace ProyectoU1.Models
{
    public class Pelicula
    {
        public int FilmId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255, ErrorMessage = "El título no puede superar 255 caracteres.")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Range(1888, 2100, ErrorMessage = "El año debe estar entre 1888 y 2100.")]
        [Display(Name = "Año de lanzamiento")]
        public int? ReleaseYear { get; set; }

        [Required(ErrorMessage = "La duración de alquiler es obligatoria.")]
        [Range(1, 7, ErrorMessage = "La duración debe ser entre 1 y 7 días.")]
        [Display(Name = "Duración del alquiler (días)")]
        public int RentalDuration { get; set; }

        [Required(ErrorMessage = "La tarifa de alquiler es obligatoria.")]
        [Display(Name = "Tarifa de alquiler")]
        [DataType(DataType.Currency)]
        public decimal RentalRate { get; set; }

        [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos.")]
        [Display(Name = "Duración (minutos)")]
        public int? Length { get; set; }

        [Required(ErrorMessage = "El costo de reemplazo es obligatorio.")]
        [Display(Name = "Costo de reemplazo")]
        [DataType(DataType.Currency)]
        public decimal ReplacementCost { get; set; }

        [RegularExpression("^(G|PG|PG-13|R|NC-17)$",
            ErrorMessage = "La clasificación debe ser G, PG, PG-13, R o NC-17.")]
        [Display(Name = "Clasificación")]
        public string? Rating { get; set; }

        public bool Activo { get; set; } = true;

        public int CopiasDisponibles { get; set; }

        public int TotalCopias { get; set; }
    }
}
