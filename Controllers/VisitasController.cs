using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Models.ViewModels;
using System.Security.Claims;

namespace PortalInmobiliario.Controllers
{
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VisitasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Visitas/Agendar/5
        [HttpGet]
        public async Task<IActionResult> Agendar(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inmueble == null)
            {
                TempData["Error"] = "El inmueble no existe o no está activo.";
                return RedirectToAction("Index", "Catalogo");
            }

            var viewModel = new AgendarVisitaViewModel
            {
                InmuebleId = id,
                Inmueble = inmueble,
                FechaInicio = DateTime.Now.AddDays(1).Date.AddHours(9), // Mañana a las 9 AM
                FechaFin = DateTime.Now.AddDays(1).Date.AddHours(10)    // Mañana a las 10 AM
            };

            return View(viewModel);
        }

        // POST: Visitas/Agendar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarVisitaViewModel model)
        {
            // Validar que el inmueble existe y está activo
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == model.InmuebleId && i.Activo);

            if (inmueble == null)
            {
                TempData["Error"] = "El inmueble no existe o no está activo.";
                return RedirectToAction("Index", "Catalogo");
            }

            // Validaciones personalizadas
            if (model.FechaInicio >= model.FechaFin)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (model.FechaInicio <= DateTime.Now)
            {
                ModelState.AddModelError("FechaInicio", "La fecha de inicio debe ser futura.");
            }

            // Validar horario laboral (8:00 - 19:00)
            var horaInicio = model.FechaInicio.TimeOfDay;
            var horaFin = model.FechaFin.TimeOfDay;

            if (horaInicio < TimeSpan.FromHours(8) || horaInicio > TimeSpan.FromHours(19) ||
                horaFin < TimeSpan.FromHours(8) || horaFin > TimeSpan.FromHours(19))
            {
                ModelState.AddModelError("", "Las visitas solo pueden agendarse en horario laboral (08:00 - 19:00).");
            }

            // Verificar visitas solapadas
            var visitasSolapadas = await _context.Visitas
                .Where(v => v.InmuebleId == model.InmuebleId && 
                           v.Estado != EstadoVisita.Cancelada &&
                           ((v.FechaInicio <= model.FechaInicio && v.FechaFin > model.FechaInicio) ||
                            (v.FechaInicio < model.FechaFin && v.FechaFin >= model.FechaFin) ||
                            (v.FechaInicio >= model.FechaInicio && v.FechaFin <= model.FechaFin)))
                .AnyAsync();

            if (visitasSolapadas)
            {
                ModelState.AddModelError("", "Ya existe una visita programada que se solapa con el horario seleccionado.");
            }

            if (!ModelState.IsValid)
            {
                model.Inmueble = inmueble;
                return View(model);
            }

            // Crear la visita
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visita = new Visita
            {
                InmuebleId = model.InmuebleId,
                UsuarioId = userId!,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Notas = model.Notas,
                Estado = EstadoVisita.Confirmada
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Visita agendada exitosamente para el {model.FechaInicio:dd/MM/yyyy HH:mm}.";
            return RedirectToAction("Detalle", "Catalogo", new { id = model.InmuebleId });
        }

        // POST: Visitas/Reservar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inmueble == null)
            {
                TempData["Error"] = "El inmueble no existe o no está activo.";
                return RedirectToAction("Index", "Catalogo");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar si ya existe una reserva activa para este inmueble
            var reservaActiva = await _context.Reservas
                .Where(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now)
                .FirstOrDefaultAsync();

            if (reservaActiva != null)
            {
                TempData["Error"] = "Este inmueble ya tiene una reserva activa.";
                return RedirectToAction("Detalle", "Catalogo", new { id });
            }

            // Crear la reserva (48 horas)
            var reserva = new Reserva
            {
                InmuebleId = id,
                UsuarioId = userId!,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddHours(48)
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inmueble reservado exitosamente por 48 horas hasta el {reserva.FechaExpiracion:dd/MM/yyyy HH:mm}.";
            return RedirectToAction("Detalle", "Catalogo", new { id });
        }
    }
}