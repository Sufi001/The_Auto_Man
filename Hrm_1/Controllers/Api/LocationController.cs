using Inventory.Helper;
using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class LocationController : ApiController
    {
        private readonly DataContext db;
        public LocationController()
        {
            db = new DataContext();
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateLocation(LOCATION locc)//, string txt_loc
        {
            try
            {
                var v = new LOCATION();
                string s = db.LOCATIONs.Max(u => u.LOC_ID);
                if (string.IsNullOrEmpty(s))
                {
                    s = "01";
                }
                v.LOC_ID =  CommonFunctions.GenerateCode(s, 2);
                v.NAME = locc.NAME;
                db.LOCATIONs.Add(v);
                db.SaveChanges();
                var _loc = db.LOCATIONs.Select(x => new { x.LOC_ID, x.NAME }).ToList().OrderBy(u => u.LOC_ID);
                return Ok(_loc);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return null;
            }
        }


    }
}
