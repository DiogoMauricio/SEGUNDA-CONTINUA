using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Imagen { get; set; }

        [Required]
        public TipoInmueble Tipo { get; set; }

        [Required]
        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Los dormitorios deben ser mayor o igual a 0")]
        public int Dormitorios { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Los baños deben ser mayor a 0")]
        public int Banos { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Los metros cuadrados deben ser mayor a 0")]
        public double MetrosCuadrados { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }

    public enum TipoInmueble
    {
        Departamento,
        Casa,
        Oficina,
        Local
    }
}