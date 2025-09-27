using Microsoft.AspNetCore.Mvc;
using PortalInmobiliario.Extensions;
using System.Text.Json;

namespace PortalInmobiliario.ViewComponents
{
    public class LastVisitedInmuebleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            try
            {
                var lastVisitedJson = HttpContext.Session.GetString("LastVisitedInmueble");
                
                if (!string.IsNullOrEmpty(lastVisitedJson))
                {
                    var inmuebleInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(lastVisitedJson);
                    if (inmuebleInfo != null && inmuebleInfo.ContainsKey("Id") && inmuebleInfo.ContainsKey("Titulo"))
                    {
                        // Manejo seguro de JsonElement
                        ViewBag.InmuebleId = GetJsonElementValue(inmuebleInfo["Id"]);
                        ViewBag.InmuebleTitulo = GetJsonElementValue(inmuebleInfo["Titulo"]);
                        return View();
                    }
                }
            }
            catch (Exception)
            {
                // Si hay cualquier error, simplemente no mostrar el componente
            }
            
            return Content("");
        }

        private object GetJsonElementValue(object value)
        {
            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => jsonElement.GetInt32(),
                    _ => value.ToString()
                };
            }
            return value;
        }
    }
}