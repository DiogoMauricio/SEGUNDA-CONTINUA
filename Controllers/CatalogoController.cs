using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Catalogo
        public async Task<IActionResult> Index(CatalogoFilterModel filtros)
        {
            // Validar filtros
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, crear el ViewModel con las ciudades para el dropdown
                var viewModel = new CatalogoViewModel
                {
                    Filtros = filtros,
                    Ciudades = await _context.Inmuebles
                        .Where(i => i.Activo)
                        .Select(i => i.Ciudad)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync()
                };
                return View(viewModel);
            }

            // Validación personalizada del rango de precios
            if (filtros.PrecioMin.HasValue && filtros.PrecioMax.HasValue && filtros.PrecioMin > filtros.PrecioMax)
            {
                ModelState.AddModelError("PrecioMin", "El precio mínimo no puede ser mayor que el precio máximo");
                ModelState.AddModelError("PrecioMax", "El precio mínimo no puede ser mayor que el precio máximo");
            }

            // Si hay errores después de la validación personalizada
            if (!ModelState.IsValid)
            {
                var viewModelError = new CatalogoViewModel
                {
                    Filtros = filtros,
                    Ciudades = await _context.Inmuebles
                        .Where(i => i.Activo)
                        .Select(i => i.Ciudad)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync()
                };
                return View(viewModelError);
            }

            var query = _context.Inmuebles.Where(i => i.Activo);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filtros.Ciudad))
            {
                query = query.Where(i => i.Ciudad.Contains(filtros.Ciudad));
            }

            if (filtros.Tipo.HasValue)
            {
                query = query.Where(i => i.Tipo == filtros.Tipo.Value);
            }

            if (filtros.PrecioMin.HasValue)
            {
                query = query.Where(i => i.Precio >= filtros.PrecioMin.Value);
            }

            if (filtros.PrecioMax.HasValue)
            {
                query = query.Where(i => i.Precio <= filtros.PrecioMax.Value);
            }

            if (filtros.DormitoriosMin.HasValue)
            {
                query = query.Where(i => i.Dormitorios >= filtros.DormitoriosMin.Value);
            }

            // Contar total de elementos
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalItems / filtros.ItemsPorPagina);

            // Aplicar paginación - convertir a lista primero para evitar problemas con SQLite y decimal
            var inmueblesList = await query.ToListAsync();
            var inmuebles = inmueblesList
                .OrderBy(i => i.Ciudad)
                .ThenBy(i => i.Precio)
                .Skip((filtros.Pagina - 1) * filtros.ItemsPorPagina)
                .Take(filtros.ItemsPorPagina)
                .ToList();

            // Obtener ciudades para el dropdown
            var ciudades = await _context.Inmuebles
                .Where(i => i.Activo)
                .Select(i => i.Ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var resultado = new CatalogoViewModel
            {
                Inmuebles = inmuebles,
                Filtros = filtros,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                Ciudades = ciudades
            };

            return View(resultado);
        }

        // GET: Catalogo/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
                return NotFound();

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (inmueble == null)
                return NotFound();

            // Verificar si hay reserva activa
            var reservaActiva = await _context.Reservas
                .Where(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now)
                .AnyAsync();

            ViewBag.ReservaActiva = reservaActiva;

            return View(inmueble);
        }
    }
}
