using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class CatalogoFilterModel
    {
        public string? Ciudad { get; set; }
        
        public TipoInmueble? Tipo { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo no puede ser negativo")]
        public decimal? PrecioMin { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo no puede ser negativo")]
        public decimal? PrecioMax { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Los dormitorios no pueden ser negativos")]
        public int? DormitoriosMin { get; set; }
        
        public int Pagina { get; set; } = 1;
        public int ItemsPorPagina { get; set; } = 10;
        
        // Validación personalizada para el rango de precios
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PrecioMin.HasValue && PrecioMax.HasValue && PrecioMin > PrecioMax)
            {
                yield return new ValidationResult(
                    "El precio mínimo no puede ser mayor que el precio máximo",
                    new[] { nameof(PrecioMin), nameof(PrecioMax) });
            }
        }
    }
    
    public class CatalogoViewModel
    {
        public IEnumerable<Inmueble> Inmuebles { get; set; } = new List<Inmueble>();
        public CatalogoFilterModel Filtros { get; set; } = new CatalogoFilterModel();
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public List<string> Ciudades { get; set; } = new List<string>();
    }
}