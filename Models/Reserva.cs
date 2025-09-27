using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PortalInmobiliario.Validators;

namespace PortalInmobiliario.Models
{
    [UnaReservaActivaPorInmueble]
    public class Reserva
    {
        public int Id { get; set; }

        [Required]
        public int InmuebleId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Expiración")]
        public DateTime FechaExpiracion { get; set; }

        [Required]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        // Navegación
        public virtual Inmueble Inmueble { get; set; } = null!;
        public virtual IdentityUser Usuario { get; set; } = null!;
    }
}