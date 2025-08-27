using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace Turnos.C.Helpers
{
    /// <summary>
    /// Extension para poder renderizar partials pasándole el prefix de binding.
    /// Esto permite reutilizar partials con submodelos anidados (por ejemplo: Direccion, Profesional, etc)
    /// y que Razor genere los name="Direccion.Calle" correctamente.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renderiza un partial con el HtmlFieldPrefix especificado.
        /// </summary>
        /// <param name="htmlHelper">HtmlHelper de la view</param>
        /// <param name="partialViewName">Ruta al partial (por ejemplo: ~/Views/Direcciones/_FormDireccion.cshtml)</param>
        /// <param name="model">El submodelo que se pasa al partial (por ejemplo: Model.Direccion)</param>
        /// <param name="prefix">El prefix de binding (por ejemplo: "Direccion")</param>
        public static Task<IHtmlContent> PartialWithPrefixAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            string prefix)
        {
            // Creamos una nueva instancia de ViewDataDictionary basada en el ViewData actual.
            var viewData = new ViewDataDictionary(htmlHelper.ViewData);

            // Indicamos el prefix de binding que Razor debe usar dentro del partial.
            viewData.TemplateInfo.HtmlFieldPrefix = prefix;

            // Ejecutamos el partial con el ViewData modificado.
            return htmlHelper.PartialAsync(partialViewName, model, viewData);
        }
    }
}
