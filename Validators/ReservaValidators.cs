using System.ComponentModel.DataAnnotations;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Validators
{
    public class UnaReservaActivaPorInmuebleAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var reserva = (Reserva)validationContext.ObjectInstance;
            var context = validationContext.GetRequiredService<ApplicationDbContext>();

            // Verificar si ya existe una reserva activa para este inmueble
            var reservaActiva = context.Reservas
                .Where(r => r.InmuebleId == reserva.InmuebleId && 
                           r.Id != reserva.Id && // Excluir la reserva actual en caso de edición
                           DateTime.Now < r.FechaExpiracion)
                .Any();

            if (reservaActiva)
            {
                return new ValidationResult("Este inmueble ya tiene una reserva activa.");
            }

            return ValidationResult.Success;
        }
    }
}