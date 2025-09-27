using System.ComponentModel.DataAnnotations;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Validators
{
    public class NoVisitasSolapadasAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime fechaFin)
                return ValidationResult.Success;

            var visita = (Visita)validationContext.ObjectInstance;
            var context = validationContext.GetRequiredService<ApplicationDbContext>();

            // Verificar si hay visitas solapadas para el mismo inmueble
            var visitasSolapadas = context.Visitas
                .Where(v => v.InmuebleId == visita.InmuebleId && 
                           v.Id != visita.Id && // Excluir la visita actual en caso de edición
                           v.Estado != EstadoVisita.Cancelada &&
                           ((v.FechaInicio <= visita.FechaInicio && v.FechaFin > visita.FechaInicio) ||
                            (v.FechaInicio < visita.FechaFin && v.FechaFin >= visita.FechaFin) ||
                            (v.FechaInicio >= visita.FechaInicio && v.FechaFin <= visita.FechaFin)))
                .Any();

            if (visitasSolapadas)
            {
                return new ValidationResult("Ya existe una visita programada para este inmueble en el horario seleccionado.");
            }

            return ValidationResult.Success;
        }
    }

    public class FechaInicioMenorQueFechaFinAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime fechaFin)
                return ValidationResult.Success;

            var visita = (Visita)validationContext.ObjectInstance;

            if (visita.FechaInicio >= fechaFin)
            {
                return new ValidationResult("La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            return ValidationResult.Success;
        }
    }
}