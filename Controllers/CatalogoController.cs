using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services;
using PortalInmobiliario.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public CatalogoController(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // GET: Catalogo
        public async Task<IActionResult> Index(CatalogoFilterModel filtros, bool limpiar = false)
        {
            // Si se solicita limpiar, crear filtros vacíos y limpiar sesión
            if (limpiar)
            {
                filtros = new CatalogoFilterModel { Pagina = 1, ItemsPorPagina = 10 };
                HttpContext.Session.Remove("LastFilters");
            }
            else
            {
                // Cargar últimos filtros de la sesión si no se han especificado nuevos
                var lastFilters = HttpContext.Session.GetObject<CatalogoFilterModel>("LastFilters");
                if (lastFilters != null && IsEmptyFilter(filtros))
                {
                    filtros = lastFilters;
                }

                // Guardar filtros actuales en sesión (solo si no están vacíos)
                if (!IsEmptyFilter(filtros))
                {
                    HttpContext.Session.SetObject("LastFilters", filtros);
                }
            }

            // Validar filtros
            if (!ModelState.IsValid)
            {
                var viewModel = await CreateViewModelAsync(filtros);
                return View(viewModel);
            }

            // Validación personalizada del rango de precios
            if (filtros.PrecioMin.HasValue && filtros.PrecioMax.HasValue && filtros.PrecioMin > filtros.PrecioMax)
            {
                ModelState.AddModelError("PrecioMin", "El precio mínimo no puede ser mayor que el precio máximo");
                ModelState.AddModelError("PrecioMax", "El precio mínimo no puede ser mayor que el precio máximo");
            }

            if (!ModelState.IsValid)
            {
                var viewModelError = await CreateViewModelAsync(filtros);
                return View(viewModelError);
            }

            // Intentar obtener desde cache
            var cacheKey = _cacheService.GenerateCacheKey(filtros);
            var cachedResult = await _cacheService.GetAsync<CatalogoViewModel>(cacheKey);
            
            if (cachedResult != null)
            {
                ViewData["FromCache"] = true;
                return View(cachedResult);
            }

            // No hay cache, consultar base de datos
            var resultado = await GetInmueblesFromDatabaseAsync(filtros);
            
            // Guardar en cache
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromSeconds(60));
            
            ViewData["FromCache"] = false;
            return View(resultado);
        }

        private bool IsEmptyFilter(CatalogoFilterModel filtros)
        {
            return string.IsNullOrEmpty(filtros.Ciudad) &&
                   !filtros.Tipo.HasValue &&
                   !filtros.PrecioMin.HasValue &&
                   !filtros.PrecioMax.HasValue &&
                   !filtros.DormitoriosMin.HasValue &&
                   filtros.Pagina == 1;
        }

        private async Task<CatalogoViewModel> CreateViewModelAsync(CatalogoFilterModel filtros)
        {
            var ciudades = await _context.Inmuebles
                .Where(i => i.Activo)
                .Select(i => i.Ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return new CatalogoViewModel
            {
                Filtros = filtros,
                Ciudades = ciudades
            };
        }

        private async Task<CatalogoViewModel> GetInmueblesFromDatabaseAsync(CatalogoFilterModel filtros)
        {
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

            // Aplicar paginación
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

            return new CatalogoViewModel
            {
                Inmuebles = inmuebles,
                Filtros = filtros,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                Ciudades = ciudades
            };
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

            // Guardar el último inmueble visitado en la sesión
            HttpContext.Session.SetString("LastVisitedInmueble", 
                System.Text.Json.JsonSerializer.Serialize(new { Id = inmueble.Id, Titulo = inmueble.Titulo }));

            // Verificar si hay reserva activa
            var reservaActiva = await _context.Reservas
                .Where(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now)
                .AnyAsync();

            ViewBag.ReservaActiva = reservaActiva;

            return View(inmueble);
        }
    }
}
