using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace Fast.Untility.Core.Base
{
    public static class BaseView
    {
        public static async Task<string> RenderViewAsync<TModel>(Controller controller,string view, TModel model,bool isMainPage=false)
        {
            controller.ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var engine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var result = engine.FindView(controller.ControllerContext, view, isMainPage);
                var viewContext = new ViewContext(controller.ControllerContext, result.View, controller.ViewData, controller.TempData, writer, new HtmlHelperOptions());
                await result.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
