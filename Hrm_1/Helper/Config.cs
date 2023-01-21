using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Inventory.Helper
{
    public class Config
    {
        public static readonly string CloudName = ConfigurationManager.AppSettings["CloudName"];
        public static readonly string CloudAPI = ConfigurationManager.AppSettings["CloudAPI"];
        public static readonly string CloudAPISecret = ConfigurationManager.AppSettings["CloudAPISecret"];
        public static readonly string CloudURL = ConfigurationManager.AppSettings["CloudURL"];
    }
}