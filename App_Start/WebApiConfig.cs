using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BizSol.Tracker.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "BizSolTrackerApi",
                routeTemplate: "api/Plugin/{controller}/{id}",
                defaults: new { controller = "Login", action = "Index", id = RouteParameter.Optional },
                new[] { "BizSol.Tracker.Api.Controllers" }
            );
        }
    }
}
