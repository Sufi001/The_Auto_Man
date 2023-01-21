using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class MeasuringUnitController : ApiController
    {
        private DataContext db;
        public MeasuringUnitController()
        {
            db = new DataContext();
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateUnit(MEARSURING_UNITS vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return null;
                }
                var v = new MEARSURING_UNITS();

                v.UNIT_NAME = vm.UNIT_NAME;
                db.MEARSURING_UNITS.Add(v);
                db.SaveChanges();
                var _loc = db.MEARSURING_UNITS.ToList().OrderBy(u => u.id);
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
