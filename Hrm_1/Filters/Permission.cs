using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Inventory.Filters
{
    public class Permission : ActionFilterAttribute
    {
        public string[] ActionRequest { get; set; }
        public Permission(string name)
        {
            var data = name.Split(',');
            ActionRequest = data;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionName = filterContext.ActionDescriptor.ActionName;
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
            HttpCookie authCookie = filterContext.HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                string UserName = ticket.Name;
                if (UserName != null)
                {
                    var permissions = ticket.UserData.Split(',');
                    var CanAccess = permissions.Any( x=> ActionRequest.Contains(x) );
                    if (!CanAccess)
                        filterContext.Result = new RedirectResult("~/AccessDenied.html");
                    //filterContext.Result = new RedirectResult("~/"+controllerName+"/"+actionName);
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/AccessDenied.html");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("~/AccessDenied.html");
            }
        }
    }
}

