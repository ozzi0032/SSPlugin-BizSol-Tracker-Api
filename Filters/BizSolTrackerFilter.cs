using SmartStore.Web.Models.Customer;
using SmartStore.Web.Controllers;
using System.Web.Mvc;
using System.Web.Routing;

namespace BizSol.Tracker.Api.Filters
{
    public class BizSolTrackerFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext) { }
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null ||
                filterContext.ActionDescriptor == null ||
                filterContext.HttpContext == null ||
                filterContext.HttpContext.Request == null)
                return;

            if (filterContext.Controller.GetType().Equals(typeof(CustomerController))
                && filterContext.ActionDescriptor.ActionName.Equals("Login")
                )
            {
                if (filterContext.HttpContext.Request.HttpMethod == "POST")
                {
                    LoginModel loginModel = filterContext.ActionParameters["model"] as LoginModel;
                    filterContext.ActionParameters.Remove("model");
                    filterContext.ActionParameters.Add("model", loginModel);

                    var _routeValues = new RouteValueDictionary(
                        new
                        {
                            controller = "Main",
                            action = "Login",
                            area = Plugin.SystemName,
                            nam = (filterContext.ActionParameters["model"] as LoginModel).UsernameOrEmail,
                            pass = (filterContext.ActionParameters["model"] as LoginModel).Password,
                            returnUrl = filterContext.ActionParameters["returnUrl"],
                        }
                        );
                    filterContext.Result = new RedirectToRouteResult("SmartStore.BizSol.Tracker", _routeValues);
                }
            }
        }

    }
}