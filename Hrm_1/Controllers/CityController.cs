using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class CityController : Controller
    {
        private DataContext db;
        public CityController()
        {
            db = new DataContext();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Save(CityViewModel city)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Model State Is Not Valid";
                    return View("Index");
                }
                var NewCity = new CITY();

                if (!string.IsNullOrEmpty(city.Id))
                {
                    NewCity = db.CITies.Where(x => x.CITY_CODE == city.Id).SingleOrDefault();
                    NewCity.UPDATED_BY = CommonFunctions.GetUserName();
                }
                else
                {
                   NewCity.CITY_CODE = GetNewCityCode();
                   NewCity.INSERTED_BY = CommonFunctions.GetUserName();
                   NewCity.DOC = DateTime.Now;
                }

                NewCity.CITY_NAME = city.Name;

                if(string.IsNullOrEmpty(city.Id))
                    db.CITies.Add(NewCity);

                db.SaveChanges();
                TempData["Success"] = "Entry Successfully Saved";
                return RedirectToAction("index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Something Bad Happend. Please Contact to Administrator";
                return View("Index");
            }
        }

        public ActionResult List()
        {
            var Vm = db.CITies.Select(x => new CityViewModel { Id = x.CITY_CODE, Name = x.CITY_NAME }).OrderBy(x => x.Id).ToList();
            return View(Vm);
        }
        public ActionResult Edit(string id)
        {
            try
            {
                var VM = db.CITies.Where(x => x.CITY_CODE == id).Select(x => new CityViewModel { Id = x.CITY_CODE, Name = x.CITY_NAME }).SingleOrDefault();
                return View("Index", VM);
            }
            catch (Exception ex)
            {
                return View("List");
            }
        }
        public string GetNewCityCode()
        {
            string s = db.CITies.Max(u => u.CITY_CODE);
            if (s == "" || s == null)
                return "0001";
            else
                return (Convert.ToInt32(s) + 1).ToString().PadLeft(4, '0');
        }

    }
}