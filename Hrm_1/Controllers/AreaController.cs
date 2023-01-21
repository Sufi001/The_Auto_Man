using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class AreaController : Controller
    {
        private DataContext db;
        public AreaController()
        {
            db = new DataContext();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Save(AreaViewModel area)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Model State Is Not Valid";
                    return View("Index");
                }
                var NewArea = new AREA();
                
                
                if (!string.IsNullOrEmpty(area.Id))
                {
                    NewArea = db.AREAs.Where(x => x.AREA_CODE == area.Id).SingleOrDefault();
                    NewArea.UPDATED_BY = CommonFunctions.GetUserName();
                }
                else
                {
                    NewArea.AREA_CODE = GetNewAreaCode();
                    NewArea.INSERTED_BY = CommonFunctions.GetUserName();
                    NewArea.DOC = DateTime.Now;
                }

                NewArea.AREA_NAME = area.Name;

                if (string.IsNullOrEmpty(area.Id))
                    db.AREAs.Add(NewArea);

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
            var Vm = db.AREAs.Select(x => new AreaViewModel { Id = x.AREA_CODE, Name = x.AREA_NAME }).OrderBy(x => x.Id).ToList();
            return View(Vm);
        }
        public ActionResult Edit(string id)
        {
            try
            {
                var VM = db.AREAs.Where(x => x.AREA_CODE == id).Select(x => new AreaViewModel { Id = x.AREA_CODE, Name = x.AREA_NAME }).SingleOrDefault();
                return View("Index", VM);
            }
            catch (Exception ex)
            {
                return View("List");
            }
        }
        public string GetNewAreaCode()
        {
            string s = db.AREAs.Max(u => u.AREA_CODE);
            if (s == "" || s == null)
                return "0001";
            else
                return (Convert.ToInt32(s) + 1).ToString().PadLeft(4, '0');
        }

    }
}