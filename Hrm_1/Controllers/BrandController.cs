using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext db;
        public BrandController()
        {
            db = new DataContext();
        }
        // GET: Design
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Save(BrandViewModel brand)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Model State Is Not Valid";
                    return View("Index");
                }
                var NewBrand = new BRAND();

                if (!string.IsNullOrEmpty(brand.Id))
                    NewBrand = db.BRANDs.Where(x => x.ID == brand.Id).SingleOrDefault();
                else
                    NewBrand.ID = GetBrandCode();



                NewBrand.NAME = brand.Name.Trim();

                if (string.IsNullOrEmpty(brand.Id))
                    db.BRANDs.Add(NewBrand);

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
            var Vm = db.BRANDs.Select(x => new BrandViewModel { Id = x.ID, Name = x.NAME }).ToList();
            return View(Vm);
        }
        public ActionResult Edit(string id)
        {
            try
            {
                var VM = db.BRANDs.Where(x => x.ID == id).Select(x => new BrandViewModel { Id = x.ID, Name = x.NAME }).SingleOrDefault();
                return View("Index", VM);
            }
            catch (Exception ex)
            {
                return View("List");
            }
        }
        public string GetBrandCode()
        {
            var maxId = db.BRANDs.Max(x => x.ID);
            if (string.IsNullOrEmpty(maxId))
                return "0001";
            else
                return CommonFunctions.GenerateCode(maxId, 4);

        }

    }
}