using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers
{
    [Authorize(Roles = "Broker")]
    public class BrokerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrokerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Broker
        public async Task<IActionResult> Index()
        {
            var stats = new BrokerDashboardViewModel
            {
                TotalInmuebles = await _context.Inmuebles.CountAsync(),
                InmueblesActivos = await _context.Inmuebles.CountAsync(i => i.Activo),
                VisitasHoy = await _context.Visitas
                    .Where(v => v.FechaInicio.Date == DateTime.Today)
                    .CountAsync(),
                ReservasActivas = await _context.Reservas
                    .Where(r => r.FechaExpiracion > DateTime.Now)
                    .CountAsync()
            };

            return View(stats);
        }

        // GET: Broker/Inmuebles
        public async Task<IActionResult> Inmuebles()
        {
            var inmuebles = await _context.Inmuebles
                .OrderBy(i => i.Codigo)
                .ToListAsync();
            return View(inmuebles);
        }

        // GET: Broker/CreateInmueble
        public IActionResult CreateInmueble()
        {
            return View();
        }

        // POST: Broker/CreateInmueble
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInmueble([Bind("Codigo,Titulo,Imagen,Tipo,Ciudad,Direccion,Dormitorios,Banos,MetrosCuadrados,Precio")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                inmueble.Activo = true;
                _context.Add(inmueble);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Inmueble creado exitosamente.";
                return RedirectToAction(nameof(Inmuebles));
            }
            return View(inmueble);
        }

        // GET: Broker/EditInmueble/5
        public async Task<IActionResult> EditInmueble(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
            {
                return NotFound();
            }
            return View(inmueble);
        }

        // POST: Broker/EditInmueble/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInmueble(int id, [Bind("Id,Codigo,Titulo,Imagen,Tipo,Ciudad,Direccion,Dormitorios,Banos,MetrosCuadrados,Precio,Activo")] Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Inmueble actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InmuebleExists(inmueble.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Inmuebles));
            }
            return View(inmueble);
        }

        // POST: Broker/ToggleInmueble/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleInmueble(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                inmueble.Activo = !inmueble.Activo;
                _context.Update(inmueble);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Inmueble {(inmueble.Activo ? "activado" : "desactivado")} exitosamente.";
            }
            return RedirectToAction(nameof(Inmuebles));
        }

        // GET: Broker/AgendaHoy
        public async Task<IActionResult> AgendaHoy()
        {
            var visitasHoy = await _context.Visitas
                .Include(v => v.Inmueble)
                .Include(v => v.Usuario)
                .Where(v => v.FechaInicio.Date == DateTime.Today)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();

            return View(visitasHoy);
        }

        // POST: Broker/ConfirmarVisita/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita != null)
            {
                visita.Estado = EstadoVisita.Confirmada;
                _context.Update(visita);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Visita confirmada exitosamente.";
            }
            return RedirectToAction(nameof(AgendaHoy));
        }

        // POST: Broker/CancelarVisita/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita != null)
            {
                visita.Estado = EstadoVisita.Cancelada;
                _context.Update(visita);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Visita cancelada exitosamente.";
            }
            return RedirectToAction(nameof(AgendaHoy));
        }

        // GET: Broker/Reservas
        public async Task<IActionResult> Reservas()
        {
            var reservasActivas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Where(r => r.FechaExpiracion > DateTime.Now)
                .OrderBy(r => r.FechaCreacion)
                .ToListAsync();

            return View(reservasActivas);
        }

        // POST: Broker/LiberarReserva/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiberarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reserva liberada exitosamente.";
            }
            return RedirectToAction(nameof(Reservas));
        }

        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.Id == id);
        }
    }

    // ViewModel para el dashboard del broker
    public class BrokerDashboardViewModel
    {
        public int TotalInmuebles { get; set; }
        public int InmueblesActivos { get; set; }
        public int VisitasHoy { get; set; }
        public int ReservasActivas { get; set; }
    }
}