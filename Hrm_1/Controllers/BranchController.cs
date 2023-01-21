using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class BranchController : Controller
    {
        private readonly DataContext db;
        public BranchController()
        {
            db = new DataContext();
        }
        // GET: Design
        public ActionResult Index()
        {
            BranchViewModel vm = new BranchViewModel();
            return View(vm);
        }
        public ActionResult Save(BranchViewModel branch)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Model State Is Not Valid";
                    return View("Index", branch);
                }
                var NewBranch = new BRANCH();

                if (!string.IsNullOrEmpty(branch.Id))
                    NewBranch = db.BRANCHes.Where(x => x.BRANCH_CODE == branch.Id).SingleOrDefault();
                else
                    NewBranch.BRANCH_CODE = GetBranhcCode();

                NewBranch.BRANCH_NAME = branch.Name.Trim();
                NewBranch.ADDRESS = branch.Address;
                NewBranch.EMAIL = branch.Email;
                NewBranch.STATUS = "a";
                NewBranch.CITY_CODE = branch.CityCode;

                if (string.IsNullOrEmpty(branch.Id))
                    db.BRANCHes.Add(NewBranch);

                db.SaveChanges();
                TempData["Success"] = "Entry Successfully Saved";
                return RedirectToAction("index", new BranchViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Something Bad Happend. Please Contact to Administrator";
                return View("Index");
            }
        }
        public ActionResult List()
        {
            var Vm = db.BRANCHes.Select(x => new BranchViewModel
            { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME, Address = x.ADDRESS, CityCode = x.CITY_CODE, Email = x.EMAIL, Status = x.STATUS }).ToList();
            return View(Vm);
        }
        public ActionResult Edit(string id)
        {
            try
            {
                var VM = db.BRANCHes.Where(x => x.BRANCH_CODE == id).Select(x => new BranchViewModel
                { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME, Address = x.ADDRESS, CityCode = x.CITY_CODE, Email = x.EMAIL, Status = x.STATUS }).SingleOrDefault();
                return View("Index", VM);
            }
            catch (Exception ex)
            {
                return View("List");
            }
        }
        public string GetBranhcCode()
        {
            var maxId = db.BRANCHes.Max(x => x.BRANCH_CODE);
            if (string.IsNullOrEmpty(maxId))
                return "01";
            else
                return CommonFunctions.GenerateCode(maxId, 2);

        }

    }
}