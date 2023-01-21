using Inventory.Helper;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;

namespace Inventory.Filters
{
    public class ApiPermission : ActionFilterAttribute
    {
        public string[] ActionRequest { get; set; }
        public ApiPermission(string name)
        {
            var data = name.Split(',');
            ActionRequest = data;
        }
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var actionName = filterContext.ActionDescriptor.ActionName;
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            HttpCookie authCookie = CommonFunctions.GetAuthCookie(); //Get the cookie by it's name
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                string UserName = ticket.Name;
                if (UserName != null)
                {
                    var permissions = ticket.UserData.Split(',');
                    var CanAccess = permissions.Any(x => ActionRequest.Contains(x));
                    if (!CanAccess)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                        string message = "You Are Not Authorize To Do This Action";
                        response.Content = new StringContent(message);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                        filterContext.Response = response;
                    }
                }
                else
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    string message = "You Are Not Authorize To Do This Action";
                    response.Content = new StringContent(message);
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    filterContext.Response = response;

                }
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                string message = "You Are Not Authorize To Do This Action";
                response.Content = new StringContent(message);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                filterContext.Response = response;
            }
        }
    }
}

