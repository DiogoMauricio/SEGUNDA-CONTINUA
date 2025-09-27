using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PortalInmobiliario.Validators;

namespace PortalInmobiliario.Models
{
    public class Visita
    {
        public int Id { get; set; }

        [Required]
        public int InmuebleId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required]
        [Display(Name = "Fecha de Fin")]
        [FechaInicioMenorQueFechaFin]
        [NoVisitasSolapadas]
        public DateTime FechaFin { get; set; }

        [Required]
        public EstadoVisita Estado { get; set; } = EstadoVisita.Solicitada;

        [StringLength(1000)]
        public string? Notas { get; set; }

        // Navegación
        public virtual Inmueble Inmueble { get; set; } = null!;
        public virtual IdentityUser Usuario { get; set; } = null!;
    }

    public enum EstadoVisita
    {
        Solicitada,
        Confirmada,
        Cancelada
    }
}