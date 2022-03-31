using BizSol.Tracker.Api.Models;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
using SmartStore.Services.Authentication;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Models.Customer;
using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace BizSol.Tracker.Api.Controllers
{
    public class MainController : PluginControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerActivityService _customerActivityService;

        public MainController(IAuthenticationService authenticationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IShoppingCartService shoppingCartService,
            ICustomerActivityService customerActivityService)
        {
            _authenticationService = authenticationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _shoppingCartService = shoppingCartService;
            _customerActivityService = customerActivityService;
        }


        [HttpGet]
        public ActionResult Index()
        {
            var model = new BizSolTrackerModel
            {
                Name = "The World's famous quotes!",
                Author = "John",
                PublishingDate = "22/2/19"
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Login([Bind(Prefix = "nam")] string username, 
            [Bind(Prefix = "pass")] string password, 
            string returnUrl)
        {
            if (_customerSettings.CustomerLoginType == CustomerLoginType.Username ||
                _customerSettings.CustomerLoginType == CustomerLoginType.Email ||
                _customerSettings.CustomerLoginType == CustomerLoginType.UsernameOrEmail &&
                username != null)
            {
                username = username.Trim();
            }

            var userNameOrEmail = String.Empty;
            if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
            {
                userNameOrEmail = username;
            }
            else if (_customerSettings.CustomerLoginType == CustomerLoginType.Username)
            {
                userNameOrEmail = username;
            }
            else
            {
                userNameOrEmail = username;
            }

            if (_customerRegistrationService.ValidateCustomer(userNameOrEmail, password))
            {
                Customer customer = null;

                if (_customerSettings.CustomerLoginType == CustomerLoginType.Email)
                {
                    customer = _customerService.GetCustomerByEmail(userNameOrEmail);
                }
                else if (_customerSettings.CustomerLoginType == CustomerLoginType.Username)
                {
                    customer = _customerService.GetCustomerByUsername(userNameOrEmail);
                }
                else
                {
                    customer = _customerService.GetCustomerByEmail(userNameOrEmail);
                    if (customer == null)
                        customer = _customerService.GetCustomerByUsername(userNameOrEmail);
                }

                _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer);

                _authenticationService.SignIn(customer, false);

                _customerActivityService.InsertActivity("PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                Services.EventPublisher.Publish(new CustomerLogedInEvent { Customer = customer });

                // Redirect home where redirect to referrer would be confusing.
                if (returnUrl.IsEmpty() || returnUrl.Contains(@"/login?") || 
                    returnUrl.Contains(@"/passwordrecoveryconfirm") || 
                    returnUrl.Contains(@"/activation"))
                {
                    return RedirectToRoute("HomePage");
                }

                return RedirectToReferrer(returnUrl);
            }
            else
            {
                return View("~/Views/Customer/Login.cshtml", "~/Views/Shared/Layouts/_Layout.cshtml", new LoginModel());
            }
            //return Json(new { username, password }, JsonRequestBehavior.AllowGet);
        }
    }
}