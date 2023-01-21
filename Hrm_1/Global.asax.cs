using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using System;

namespace Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //protected void Application_BeginRequest(Object sender, EventArgs e)
        //    {
        //        CultureInfo newCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        //        newCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy hh:mm:ss tt";
        //        Thread.CurrentThread.CurrentCulture = newCulture;
        //    }
        protected void Application_BeginRequest()
        {
            CultureInfo cInf = new CultureInfo("en-US", false);
            // NOTE: change the culture name en-ZA to whatever culture suits your needs
            cInf.DateTimeFormat.DateSeparator = "/";
            cInf.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            cInf.DateTimeFormat.ShortTimePattern = "hh:mm tt";
            cInf.DateTimeFormat.LongDatePattern = "dd/MM/yyyy";
            cInf.DateTimeFormat.LongTimePattern = "hh:mm:ss tt";
            cInf.DateTimeFormat.FullDateTimePattern = "dd/MM/yyyy hh:mm:ss tt";
            Thread.CurrentThread.CurrentCulture = cInf;
            Thread.CurrentThread.CurrentUICulture = cInf;
        }
    }
}
