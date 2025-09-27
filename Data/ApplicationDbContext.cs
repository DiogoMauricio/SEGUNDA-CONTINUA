using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración del modelo Inmueble
            modelBuilder.Entity<Inmueble>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MetrosCuadrados).HasColumnType("decimal(10,2)");
            });

            // Configuración del modelo Visita
            modelBuilder.Entity<Visita>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Inmueble)
                    .WithMany(i => i.Visitas)
                    .HasForeignKey(e => e.InmuebleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Validación: FechaInicio < FechaFin usando ToTable
                entity.ToTable(t => t.HasCheckConstraint("CK_Visita_FechaInicio_FechaFin", 
                    "FechaInicio < FechaFin"));
            });

            // Configuración del modelo Reserva
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Inmueble)
                    .WithMany(i => i.Reservas)
                    .HasForeignKey(e => e.InmuebleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Semilla de datos
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inmueble>().HasData(
                new Inmueble
                {
                    Id = 1,
                    Codigo = "DEPT-001",
                    Titulo = "Departamento Moderno en Centro",
                    Imagen = "https://via.placeholder.com/400x300/0066CC/FFFFFF?text=Departamento",
                    Tipo = TipoInmueble.Departamento,
                    Ciudad = "Santiago",
                    Direccion = "Av. Providencia 1234, Providencia",
                    Dormitorios = 2,
                    Banos = 2,
                    MetrosCuadrados = 75.5,
                    Precio = 2500000,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 2,
                    Codigo = "CASA-002",
                    Titulo = "Casa Familiar con Jardín",
                    Imagen = "https://via.placeholder.com/400x300/228B22/FFFFFF?text=Casa",
                    Tipo = TipoInmueble.Casa,
                    Ciudad = "Las Condes",
                    Direccion = "Los Leones 456, Las Condes",
                    Dormitorios = 4,
                    Banos = 3,
                    MetrosCuadrados = 180.0,
                    Precio = 4200000,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 3,
                    Codigo = "OFIC-003",
                    Titulo = "Oficina Ejecutiva Torre Corporativa",
                    Imagen = "https://via.placeholder.com/400x300/4B0082/FFFFFF?text=Oficina",
                    Tipo = TipoInmueble.Oficina,
                    Ciudad = "Santiago Centro",
                    Direccion = "Av. Apoquindo 789, Las Condes",
                    Dormitorios = 0,
                    Banos = 2,
                    MetrosCuadrados = 120.0,
                    Precio = 1800000,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 4,
                    Codigo = "LOCAL-004",
                    Titulo = "Local Comercial en Mall",
                    Imagen = "https://via.placeholder.com/400x300/FF6347/FFFFFF?text=Local",
                    Tipo = TipoInmueble.Local,
                    Ciudad = "Maipú",
                    Direccion = "Av. Américo Vespucio 321, Maipú",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 45.5,
                    Precio = 950000,
                    Activo = true
                }
            );
        }
    }
}