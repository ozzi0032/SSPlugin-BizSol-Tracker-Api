using SmartStore.Web.Framework.Routing;
using System.Web.Mvc;
using System.Web.Routing;

namespace BizSol.Tracker.Api
{
    public class RouteProvider : IRouteProvider
    {

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.BizSol.Tracker",
                "Plugins/BizSolTracker/{action}/{id}",
                new { controller = "Main", action = "Index", id = UrlParameter.Optional},
                new[] { "BizSol.Tracker.Api.Controllers" }
           )
           .DataTokens["area"] = Plugin.SystemName;


           // routes.MapRoute("SmartStore.BizSol.Tracker",
           //     "Plugins/BizSolTracker/Api/{action}/{id}",
           //     new { controller = "Main", action = "Login" , id = "Empty"},
           //     new[] { "BizSol.Tracker.Api.Controllers" }
           //)
           //.DataTokens["area"] = Plugin.SystemName;
        }



        public int Priority => 0;
    }
}