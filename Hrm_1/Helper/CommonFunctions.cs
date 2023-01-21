using Inventory;
using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Http;

namespace Inventory.Helper
{
    public static class CommonFunctions
    {
        public static string GenerateCode(string s, int len)
        {
            return (Convert.ToInt32(s) + 1).ToString().PadLeft(len, '0');
        }
        public static string PadRight(string s, int len)
        {
            return s.PadRight(len, '0');
        }

        public static string GetUserName()
        {
            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName]; //Get the cookie by it's name
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                return ticket.Name.Trim();
            }
            else
                return "";
        }
        public static string GetUserID()
        {
            HttpCookie userId = HttpContext.Current.Request.Cookies["UserSettings"];
            if (userId != null)
                return userId.Value;

            else
                return "";
        }
        public static DateTime? StringToDateTime(string datetime)
        {
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime result = DateTime.Now;
                var converted = DateTime.TryParse(datetime, out result);
                if (converted)
                    return result;
                else
                    return null;
            }
            return null;
        }
        public static string DateTimeToString(DateTime? datetime)
        {
            if (datetime.HasValue)
            {
                return datetime.Value.ToString("dd/MM/yyyy h:mm:ss tt");
            }
            else
                return null;
        }
        public static String ConvertAmount(double amount)
        {
            try
            {
                Int64 amount_int = (Int64)amount;
                Int64 amount_dec = (Int64)Math.Round((amount - (double)(amount_int)) * 100);
                if (amount_dec == 0)
                {
                    return Conversion(amount_int) + " Only.";
                }
                else
                {
                    return Conversion(amount_int) + " Point " + Conversion(amount_dec) + " Only.";
                }
            }
            catch (Exception e)
            {
                // TODO: handle exception  
            }
            return "";
        }
        public static String Conversion(Int64 i)
        {
            String[] units = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
            "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            String[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (i < 20)
            {
                return units[i];
            }
            if (i < 100)
            {
                return tens[i / 10] + ((i % 10 > 0) ? " " + Conversion(i % 10) : "");
            }
            if (i < 1000)
            {
                return units[i / 100] + " Hundred" + ((i % 100 > 0) ? " And " + Conversion(i % 100) : "");
            }
            if (i < 100000)
            {
                return Conversion(i / 1000) + " Thousand " + ((i % 1000 > 0) ? " " + Conversion(i % 1000) : "");
            }
            if (i < 10000000)
            {
                return Conversion(i / 100000) + " Lakh " + ((i % 100000 > 0) ? " " + Conversion(i % 100000) : "");
            }
            if (i < 1000000000)
            {
                return Conversion(i / 10000000) + " Crore " + ((i % 10000000 > 0) ? " " + Conversion(i % 10000000) : "");
            }
            return Conversion(i / 1000000000) + " Arab " + ((i % 1000000000 > 0) ? " " + Conversion(i % 1000000000) : "");
        }
        public static string GetLocation()
        {
            HttpCookie Location = HttpContext.Current.Request.Cookies["Location"];
            if (Location != null)
                return Location.Value;

            else
                return "";            
        }

        public static HttpCookie GetAuthCookie()
        {
            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName]; //Get the cookie by it's name
            if (authCookie != null)
            {
                return authCookie;
            }
            else
                return null;
        }

        public static FormsAuthenticationTicket GetAuthCookieTicket()
        {
            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName]; //Get the cookie by it's name
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                return ticket;
            }
            else
                return null;
        }
        public static DateTime GetDateTimeNow()
        {
            DateTime UtcNow = DateTime.UtcNow;

            TimeZoneInfo PkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime DateTimeNow = TimeZoneInfo.ConvertTime(UtcNow, PkTimeZone);

            return DateTimeNow;
        }
    }
}