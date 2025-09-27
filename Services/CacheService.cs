using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Models;
using System.Text.Json;

namespace PortalInmobiliario.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        string GenerateCacheKey(CatalogoFilterModel filtros);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                if (value == null)
                    return default;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions();
                
                if (expiry.HasValue)
                    options.SetAbsoluteExpiration(expiry.Value);
                else
                    options.SetAbsoluteExpiration(TimeSpan.FromSeconds(60)); // Default 60 segundos

                await _distributedCache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // Redis no tiene una forma nativa de eliminar por patrón desde IDistributedCache
            // Por simplicidad, en este caso invalidaremos todo el cache relacionado con inmuebles
            // En una implementación real, usaríamos ConnectionMultiplexer directamente
            try
            {
                _logger.LogInformation("Cache pattern invalidation requested: {Pattern}", pattern);
                // Para este demo, simplemente loggeamos que se debe invalidar el cache
                // En producción se implementaría con Redis KEYS o mejor, con tags
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        public string GenerateCacheKey(CatalogoFilterModel filtros)
        {
            var keyComponents = new List<string>
            {
                "inmuebles",
                filtros.Ciudad ?? "null",
                filtros.Tipo?.ToString() ?? "null",
                filtros.PrecioMin?.ToString() ?? "null",
                filtros.PrecioMax?.ToString() ?? "null",
                filtros.DormitoriosMin?.ToString() ?? "null",
                filtros.Pagina.ToString(),
                filtros.ItemsPorPagina.ToString()
            };

            return string.Join(":", keyComponents);
        }
    }
}