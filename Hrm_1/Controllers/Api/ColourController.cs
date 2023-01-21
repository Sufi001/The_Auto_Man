using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class ColourController : ApiController
    {
        private readonly DataContext db;
        public ColourController()
        {
            db = new DataContext();
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateColour(COLOUR color)//, string txt_loc
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return null;
                }
                var v = new COLOUR();
                v.COLOUR_NAME = color.COLOUR_NAME;
                db.COLOURS.Add(v);
                db.SaveChanges();
                var colorList = db.COLOURS.ToList().OrderBy(u => u.id);
                return Ok(colorList);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return null;
            }
        }
    }
}
