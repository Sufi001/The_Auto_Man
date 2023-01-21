using Inventory.Filters;
using Inventory.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class LocationController : Controller
    {
        DataContext db;
        public LocationController()
        {
            db = new DataContext();
        }
        [Permission("Locations")]
        public ActionResult Index()
        {
            ViewBag.LocationList = db.LOCATIONs.ToList();
            ViewBag.CityList = db.CITies.ToList();
            ViewBag.AreaList =null;
            return View();
        }
        [HttpPost]
        public ActionResult SaveLocation(LOCATION loc)
        {
            try
            {
                var Exist = db.LOCATIONs.Any(x => x.NAME == loc.NAME);
                if (!Exist)
                {
                    var v = new LOCATION();
                    var locId = db.LOCATIONs.Max(x => x.LOC_ID);
                    if (string.IsNullOrEmpty(locId))
                    {
                        locId = "01";
                    }
                    else
                    {
                        locId =  CommonFunctions.GenerateCode(locId, 2);
                    }
                    v.LOC_ID = locId;
                    v.NAME = loc.NAME;
                    db.LOCATIONs.Add(v);
                    db.SaveChanges();
                    return Content("ok");
                }
                else
                {
                    return Content("exist");
                }
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        [HttpPost]
        public ActionResult SaveCity(CITY loc)
        {
            try
            {
                var Exist = db.CITies.Any(x => x.CITY_NAME == loc.CITY_NAME);
                if (!Exist)
                {
                    var v = new CITY();
                    var locId = db.CITies.Max(x => x.CITY_CODE);
                    if (string.IsNullOrEmpty(locId))
                    {
                        locId = "0001";
                    }
                    else
                    {
                        locId = CommonFunctions.GenerateCode(locId, 4);
                    }
                    v.CITY_CODE = locId;
                    v.CITY_NAME = loc.CITY_NAME;
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.DOC = DateTime.Now;
                    db.CITies.Add(v);
                    db.SaveChanges();
                    return Content(locId);
                }
                else
                {
                    return Content("exist");
                }
            }
            catch (Exception ex)
            {
                return Content("ex");
            }
        }
        [HttpPost]
        public ActionResult SaveArea(AREA loc)
        {
            try
            {
                var Exist = db.AREAs.Any(x => x.AREA_NAME == loc.AREA_NAME);
                if (!Exist)
                {
                    var v = new AREA();
                    var locId = db.AREAs.Max(x => x.AREA_CODE);
                    if (string.IsNullOrEmpty(locId))
                    {
                        locId = "0001";
                    }
                    else
                    {
                        locId = CommonFunctions.GenerateCode(locId, 4);
                    }
                    v.AREA_CODE = locId;
                    v.AREA_NAME = loc.AREA_NAME;
                    v.CITY_CODE = loc.CITY_CODE;
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.DOC = DateTime.Now;
                    db.AREAs.Add(v);
                    db.SaveChanges();
                    return Content(locId);
                }
                else
                {
                    return Content("exist");
                }
            }
            catch (Exception ex)
            {
                return Content("ex");

            }
        }
        [HttpPost]
        public ActionResult UpdateLocation(LOCATION loc)
        {
            try
            {
                var v = db.LOCATIONs.Where(x => x.LOC_ID == loc.LOC_ID).FirstOrDefault();
                v.NAME = loc.NAME;
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        [HttpPost]
        public ActionResult UpdateCity(CITY loc)
        {
            try
            {
                var v = db.CITies.Where(x => x.CITY_CODE == loc.CITY_CODE).FirstOrDefault();
                v.CITY_NAME = loc.CITY_NAME;
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = DateTime.Now;
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        [HttpPost]
        public ActionResult UpdateArea(AREA loc)
        {
            try
            {
                var v = db.AREAs.Where(x => x.AREA_CODE == loc.AREA_CODE).FirstOrDefault();
                v.AREA_NAME = loc.AREA_NAME;
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = DateTime.Now;
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        public JsonResult CityAreaList(string id)
        {
            JsonResult jsonResult = null;
            var group = db.AREAs.Where(x => x.CITY_CODE == id).Select(x=> new {x.AREA_CODE,x.AREA_NAME}).ToList();
            return jsonResult = Json(group, JsonRequestBehavior.AllowGet);

        }
    }
}