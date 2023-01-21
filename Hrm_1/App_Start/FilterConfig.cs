using System.Web.Mvc;
using System.Web.Services.Description;

namespace Hrm_1.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
           
        }
    }
}