using System;
using System.Net;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi.Caching;
using BizSol.Tracker.Api.Providers;
using System.Collections.Generic;
using SmartStore;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Data;
using SmartStore.Web.Framework.WebApi.Security;
using System.Net.Http;
using System.Threading.Tasks;
using SmartStore.Web.Framework.WebApi;
using System.Web;

namespace BizSol.Tracker.Api.Controllers
{
    public class LoginController : ApiController
    {
        protected HmacAuthentication _hmac = new HmacAuthentication();

        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IRepository<GenericAttribute> _genericAttributes;

        public LoginController(
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IRepository<GenericAttribute> genericAttributes
            )
        {
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _genericAttributes = genericAttributes;
            
        }

        public string[] GetCustomerAttributes(int custId)
        {
            var attribute = (
                    from a in _genericAttributes.Table
                    where a.EntityId == custId && a.KeyGroup == "Customer" && a.Key == WebApiCachingUserData.Key
                    select a).FirstOrDefault();

            //to get the public and secret key from WebApiCachingUserData aka ---> Keys exposed by Web Api Plugin
            if (!string.IsNullOrEmpty(attribute.Value))
            {
                string[] arr = attribute.Value.SplitSafe("¶");
                if (arr.Length > 2)
                {
                    return arr;
                }
            }
            return null;
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

                //Get Keys
                var arr = GetCustomerAttributes(customer.Id);
                var publicKey = arr[1];
                var secretKey = arr[2];
                var url = $"http://localhost:58201/api/Plugin/Login?order=1";

                //Generate Token
                //var signatures = AppAuthProvider.GetAuthorizationToken(publicKey,secretKey,"","get",url);

                //Below is how server side is generating the tokens
                var timeStamp = DateTime.UtcNow.ToString("o");
                var context = new WebApiRequestContext
                {
                    HttpMethod = "get",
                    HttpAcceptType = "application/json, text/javascript, */*",
                    PublicKey = publicKey,
                    SecretKey = secretKey,
                    Url = HttpUtility.UrlDecode(new Uri(url).Query).ToLower(),
                };
                var messageRepresentation = _hmac.CreateMessageRepresentation(context, null, timeStamp);
                var signatures = _hmac.CreateSignature(secretKey, messageRepresentation);

                //This is returning JSON String
                var JsonCustomerModel = JsonConvert.SerializeObject(customer, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                Dictionary<string, dynamic> response = new Dictionary<string, dynamic>();
                response.Add("token", signatures);
                response.Add("timeStamp", timeStamp);
                response.Add("publicKey", publicKey);
                response.Add("customer", JsonCustomerModel);
                return Json(response);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        [HttpGet]
        [WebApiAuthenticate]
        public IHttpActionResult GetOrders(int order)
        {
            return Json(new { order });
        }

        //public async Task MakeHttpReqAsync(string url,string publicKey,string signatures)
        //{
        //    var httpClient = new HttpClient();
        //    var msg = new HttpRequestMessage(HttpMethod.Get, url);

        //    string _time = DateTime.UtcNow.ToString("o"); // 2022-01-16T19:44:04.9378268Z
        //    msg.Headers.Add("SmartStore-Net-Api-PublicKey", publicKey);
        //    msg.Headers.Add("SmartStore-Net-Api-Date", _time);
        //    msg.Headers.Add("Authorization", "SmNetHmac1 " + signatures);
        //    msg.Headers.Add("Accept-Charset", "UTF-8");
        //    msg.Headers.Add("Accept", "application/json, text/javascript, */*");
        //    msg.Headers.Add("Content-Type", "application/json");
        //    var res = await httpClient.SendAsync(msg);

        //    Console.WriteLine(res);
        //}

    }
}
