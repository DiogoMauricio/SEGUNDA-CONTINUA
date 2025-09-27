using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models.ViewModels
{
    public class AgendarVisitaViewModel
    {
        public int InmuebleId { get; set; }
        
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(9);
        
        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [Display(Name = "Fecha de fin")]
        public DateTime FechaFin { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(10);
        
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        [Display(Name = "Notas (opcional)")]
        public string? Notas { get; set; }
        
        // Para mostrar información del inmueble en el formulario
        public Inmueble? Inmueble { get; set; }
    }
}