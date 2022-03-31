using System;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
using SmartStore.Services.Authentication;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;

namespace BizSol.Tracker.Api.Controllers
{
    public class LoginController : ApiController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerActivityService _customerActivityService;

        public LoginController(
            IAuthenticationService authenticationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IShoppingCartService shoppingCartService,
            ICustomerActivityService customerActivityService
            )
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
        public IHttpActionResult Index()
        {
            return Json(new { Message = "Route not defined!!!" });
        }

        [HttpGet]
        public IHttpActionResult PerformLogin(string username, string password)
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
                //This is returning JSON String -> later have to work on this
                var JsonCustomerModel = JsonConvert.SerializeObject(customer, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Json(JsonCustomerModel);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

    }
}
