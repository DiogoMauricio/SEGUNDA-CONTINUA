using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Crear roles si no existen
            await SeedRolesAsync(roleManager);
            
            // Crear usuarios si no existen
            await SeedUsersAsync(userManager);
            
            // Crear inmuebles si no existen
            SeedInmuebles(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Broker", "Cliente" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
        {
            // Crear usuario Broker
            var brokerEmail = "broker@portal.com";
            if (await userManager.FindByEmailAsync(brokerEmail) == null)
            {
                var brokerUser = new IdentityUser
                {
                    UserName = brokerEmail,
                    Email = brokerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(brokerUser, "Broker123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(brokerUser, "Broker");
                }
            }

            // Crear usuario Cliente
            var clienteEmail = "cliente@portal.com";
            if (await userManager.FindByEmailAsync(clienteEmail) == null)
            {
                var clienteUser = new IdentityUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(clienteUser, "Cliente123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(clienteUser, "Cliente");
                }
            }
        }

        public static void SeedData(ApplicationDbContext context)
        {
            SeedInmuebles(context);
        }

        private static void SeedInmuebles(ApplicationDbContext context)
        {
            // Verificar si ya hay datos
            if (context.Inmuebles.Any())
            {
                return; // La base de datos ya tiene datos
            }

            // Crear inmuebles de prueba
            var inmuebles = new List<Inmueble>
            {
                new Inmueble
                {
                    Codigo = "CASA001",
                    Titulo = "Casa Moderna en el Centro",
                    Direccion = "Av. Principal 123",
                    Ciudad = "Santiago",
                    Tipo = TipoInmueble.Casa,
                    Precio = 850000,
                    MetrosCuadrados = 120,
                    Dormitorios = 3,
                    Banos = 2,
                    Imagen = "https://images.unsplash.com/photo-1558618047-67d39154e5f2?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "DEPTO001",
                    Titulo = "Departamento con Vista al Mar",
                    Direccion = "Costanera Norte 456",
                    Ciudad = "Valparaíso",
                    Tipo = TipoInmueble.Departamento,
                    Precio = 650000,
                    MetrosCuadrados = 85,
                    Dormitorios = 2,
                    Banos = 2,
                    Imagen = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "CASA002",
                    Titulo = "Casa Familiar con Jardín",
                    Direccion = "Los Rosales 789",
                    Ciudad = "Concepción",
                    Tipo = TipoInmueble.Casa,
                    Precio = 420000,
                    MetrosCuadrados = 150,
                    Dormitorios = 4,
                    Banos = 3,
                    Imagen = "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "DEPTO002",
                    Titulo = "Loft Moderno en Providencia",
                    Direccion = "Providencia 321",
                    Ciudad = "Santiago",
                    Tipo = TipoInmueble.Departamento,
                    Precio = 380000,
                    MetrosCuadrados = 60,
                    Dormitorios = 1,
                    Banos = 1,
                    Imagen = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "CASA003",
                    Titulo = "Casa de Campo con Piscina",
                    Direccion = "Camino Rural 147",
                    Ciudad = "La Serena",
                    Tipo = TipoInmueble.Casa,
                    Precio = 720000,
                    MetrosCuadrados = 200,
                    Dormitorios = 5,
                    Banos = 4,
                    Imagen = "https://images.unsplash.com/photo-1523217582562-09d0def993a6?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "APART001",
                    Titulo = "Apartamento Estudiantil",
                    Direccion = "Universidad 654",
                    Ciudad = "Valparaíso",
                    Tipo = TipoInmueble.Departamento,
                    Precio = 180000,
                    MetrosCuadrados = 35,
                    Dormitorios = 1,
                    Banos = 1,
                    Imagen = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "CASA004",
                    Titulo = "Casa Colonial Restaurada",
                    Direccion = "Centro Histórico 888",
                    Ciudad = "Concepción",
                    Tipo = TipoInmueble.Casa,
                    Precio = 520000,
                    MetrosCuadrados = 180,
                    Dormitorios = 3,
                    Banos = 2,
                    Imagen = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=400",
                    Activo = true
                },
                new Inmueble
                {
                    Codigo = "DEPTO003",
                    Titulo = "Penthouse de Lujo",
                    Direccion = "Las Condes 999",
                    Ciudad = "Santiago",
                    Tipo = TipoInmueble.Departamento,
                    Precio = 1200000,
                    MetrosCuadrados = 250,
                    Dormitorios = 4,
                    Banos = 4,
                    Imagen = "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=400",
                    Activo = true
                }
            };

            context.Inmuebles.AddRange(inmuebles);
            context.SaveChanges();
        }
    }
}